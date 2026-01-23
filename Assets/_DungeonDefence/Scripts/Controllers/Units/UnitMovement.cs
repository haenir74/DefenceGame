using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Panex.Stat.Controller;
using Panex.Stat.Model;

public class UnitMovement
{
    private readonly Unit unit;
    private GridSystem gridSystem;
    private StatController statController;
    private Stat moveSpeedStat;

    private Node currentNode;
    private Node nextNode;
    private Node allyDestination;

    private readonly List<Node> visitedNodes = new List<Node>();
    private const int MAX_HISTORY = 10;

    private enum MovePhase { ToEdge, ToStance, AtStance }
    private MovePhase phase = MovePhase.ToEdge;

    public Node CurrentNode => currentNode;
    public bool HasPendingMove => nextNode != null;

    public UnitMovement(Unit unit)
    {
        this.unit = unit;
    }

    public void Initialize()
    {
        if (GridManager.Instance != null)
        {
            gridSystem = GridManager.Instance.GridSystem;
        }

        statController = unit.GetComponent<StatController>();
        if (statController != null)
        {
            moveSpeedStat = statController.GetStat(StatKeys.MoveSpeed);
        }
    }

    public void ResetPath()
    {
        nextNode = null;
        phase = MovePhase.ToEdge;
    }

    public void SetNode(Node node)
    {
        HandleNodeExit(currentNode);
        currentNode = node;
        HandleNodeEnter(currentNode);

        phase = MovePhase.ToEdge;
        visitedNodes.Clear();
        visitedNodes.Add(node);
    }

    public void DecideNextMove()
    {
        if (gridSystem == null || currentNode == null) return;
        if (nextNode != null) return;

        nextNode = (unit.MyTeam == Team.Ally) ? CalculateAllyPath() : CalculateEnemyPath();

        if (nextNode != null)
        {
            AddToHistory(nextNode);
            phase = MovePhase.ToEdge;
        }
    }

    private Node CalculateEnemyPath()
    {
        var map = GameManager.Instance.Context.map;
        if (map == null || map.CoreNode == null) return null;

        Vector3 corePos = map.CoreNode.WorldPosition;
        Node bestNode = null;
        float bestScore = float.MinValue;

        foreach (var neighbor in gridSystem.GetNeighbors(map, currentNode))
        {
            float dist = Vector3.Distance(neighbor.WorldPosition, corePos);
            int attractiveness = neighbor.GetAttractiveness();
            float penalty = visitedNodes.Contains(neighbor) ? 1000f : 0f;

            float score = (-dist * 2.0f) + attractiveness - penalty;

            if (score > bestScore)
            {
                bestScore = score;
                bestNode = neighbor;
            }
        }
        return bestNode;
    }

    private Node CalculateAllyPath()
    {
        if (allyDestination == null) return null;

        var map = GameManager.Instance.Context.map;
        Node bestNode = null;
        float minDist = float.MaxValue;

        foreach (var neighbor in gridSystem.GetNeighbors(map, currentNode))
        {
            float d = Vector3.Distance(neighbor.WorldPosition, allyDestination.WorldPosition);
            if (d < minDist)
            {
                minDist = d;
                bestNode = neighbor;
            }
        }
        return bestNode;
    }

    private void AddToHistory(Node node)
    {
        visitedNodes.Add(node);
        if (visitedNodes.Count > MAX_HISTORY) visitedNodes.RemoveAt(0);
    }

    public void MoveTowards(Vector3 targetPos)
    {
        float speed = unit.GetComponent<Panex.Stat.Controller.StatController>().GetValue("MoveSpeed");
        targetPos.y = unit.transform.position.y;
        unit.transform.position = Vector3.MoveTowards(unit.transform.position, targetPos, speed * Time.deltaTime);
    }

    public void UpdateStance()
    {
        if (currentNode == null || gridSystem == null) return;
        Vector3 flowDir = Vector3.forward;
        var map = GameManager.Instance.Context.map;

        if (nextNode != null)
        {
            flowDir = (nextNode.WorldPosition - currentNode.WorldPosition).normalized;
        }
        else
        {
            Node target = (unit.MyTeam == Team.Enemy) ? map.CoreNode : map.SpawnNode;
            if (target != null && target != currentNode)
            {
                flowDir = (target.WorldPosition - currentNode.WorldPosition).normalized;
            }
        }

        float cellSize = GridManager.Instance.Data.cellSize;
        Vector3 stancePos = gridSystem.GetStancePosition(currentNode, unit.MyTeam, flowDir, cellSize);
        
        MoveTowards(stancePos);
    }

    public bool Tick()
    {
        if (nextNode == null) return true;

        float currentMoveSpeed = moveSpeedStat != null ? moveSpeedStat.Value : 2.0f;
        float cellSize = GridManager.Instance.Data.cellSize;
        Vector3 flowDir = (nextNode.WorldPosition - currentNode.WorldPosition).normalized;
        Vector3 targetPos = unit.transform.position;

        switch (phase)
        {
            case MovePhase.ToEdge:
                targetPos = gridSystem.GetEdgePosition(currentNode, nextNode);
                break;
            case MovePhase.ToStance:
                targetPos = gridSystem.GetStancePosition(nextNode, unit.MyTeam, flowDir, cellSize);
                break;
            case MovePhase.AtStance:
                nextNode = null;
                return true;
        }

        targetPos.y = unit.transform.position.y;
        
        unit.transform.position = Vector3.MoveTowards(unit.transform.position, targetPos, currentMoveSpeed * Time.deltaTime);

        if (Vector3.SqrMagnitude(unit.transform.position - targetPos) < 0.0025f)
        {
            AdvancePhase();
        }

        return false;
    }

    private void AdvancePhase()
    {
        if (phase == MovePhase.ToEdge)
        {
            HandleNodeExit(currentNode);
            currentNode = nextNode;
            HandleNodeEnter(currentNode);
            phase = MovePhase.ToStance;
        }
        else if (phase == MovePhase.ToStance)
        {
            phase = MovePhase.AtStance;
        }
    }

    private void HandleNodeExit(Node node)
    {
        if (node != null)
        {
            node.TileEffect?.OnUnitExit(unit);
            node.UnitsOnNode.Remove(unit);
        }
    }

    private void HandleNodeEnter(Node node)
    {
        if (node != null)
        {
            node.TileEffect?.OnUnitEnter(unit);
            if (!node.UnitsOnNode.Contains(unit)) node.UnitsOnNode.Add(unit);
            CheckForBattle(node);
        }
    }

    private void CheckForBattle(Node node)
    {
        foreach (var other in node.UnitsOnNode)
        {
            if (other == unit || other.IsDead || other.MyTeam == unit.MyTeam) continue;

            if (!(unit.CurrentState is UnitBattleState))
            {
                unit.ChangeState(new UnitBattleState(unit, other));
            }

            if (!(other.CurrentState is UnitBattleState))
            {
                other.ChangeState(new UnitBattleState(other, unit));
            }

            return;
        }
    }

    public void SetAllyDestination(Node dest) => allyDestination = dest;
}