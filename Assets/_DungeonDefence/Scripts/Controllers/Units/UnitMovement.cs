using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Panex.Stat.Controller;
using Panex.Stat.Model;

public class UnitMovement
{
    private readonly Unit _unit;
    private GridSystem _gridSystem;
    private StatController _statController;
    private Stat _moveSpeedStat;

    private Node _currentNode;
    private Node _nextNode;
    private Node _allyDestination;

    private readonly List<Node> _visitedNodes = new List<Node>();
    private const int MAX_HISTORY = 10;

    private enum MovePhase { ToEdge, ToStance, AtStance }
    private MovePhase _phase = MovePhase.ToEdge;

    public Node CurrentNode => _currentNode;
    public bool HasPendingMove => _nextNode != null;

    public UnitMovement(Unit unit)
    {
        _unit = unit;
        if (GridManager.Instance != null)
            _gridSystem = GridManager.Instance.GridSystem;
        
        _statController = unit.GetComponent<StatController>();
    }

    public void Initialize()
    {
        if (_statController != null)
        {
            _moveSpeedStat = _statController.GetStat(StatKeys.MoveSpeed);
        }
    }

    public void SetNode(Node node)
    {
        HandleNodeExit(_currentNode);
        _currentNode = node;
        HandleNodeEnter(_currentNode);

        _phase = MovePhase.ToEdge;
        _visitedNodes.Clear();
        _visitedNodes.Add(node);
    }

    public void DecideNextMove()
    {
        if (_gridSystem == null || _currentNode == null) return;
        _nextNode = (_unit.MyTeam == Team.Ally) ? CalculateAllyPath() : CalculateEnemyPath();

        if (_nextNode != null)
        {
            AddToHistory(_nextNode);
            _phase = MovePhase.ToEdge;
        }
    }

    private Node CalculateEnemyPath()
    {
        var map = GameManager.Instance.Context.map;
        Vector3 corePos = map.CoreNode.WorldPosition;
        
        Node bestNode = null;
        float bestScore = float.MinValue;

        foreach (var neighbor in _gridSystem.GetNeighbors(map, _currentNode))
        {
            float dist = Vector3.Distance(neighbor.WorldPosition, corePos);
            int attractiveness = neighbor.GetAttractiveness();
            float penalty = _visitedNodes.Contains(neighbor) ? 1000f : 0f;

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
        if (_allyDestination == null) return null;

        var map = GameManager.Instance.Context.map;
        Node bestNode = null;
        float minDist = float.MaxValue;

        foreach (var neighbor in _gridSystem.GetNeighbors(map, _currentNode))
        {
            float d = Vector3.Distance(neighbor.WorldPosition, _allyDestination.WorldPosition);
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
        _visitedNodes.Add(node);
        if (_visitedNodes.Count > MAX_HISTORY) _visitedNodes.RemoveAt(0);
    }

    public bool Tick()
    {
        if (_nextNode == null) return true;

        float currentMoveSpeed = _moveSpeedStat != null ? _moveSpeedStat.Value : 2.0f;
        
        float cellSize = GridManager.Instance.Data.cellSize;
        Vector3 flowDir = (_nextNode.WorldPosition - _currentNode.WorldPosition).normalized;
        Vector3 targetPos = _unit.transform.position;

        switch (_phase)
        {
            case MovePhase.ToEdge:
                targetPos = _gridSystem.GetEdgePosition(_currentNode, _nextNode);
                break;
            case MovePhase.ToStance:
                targetPos = _gridSystem.GetStancePosition(_nextNode, _unit.MyTeam, flowDir, cellSize);
                break;
            case MovePhase.AtStance:
                return true;
        }

        targetPos.y = _unit.transform.position.y;
        
        _unit.transform.position = Vector3.MoveTowards(_unit.transform.position, targetPos, currentMoveSpeed * Time.deltaTime);

        if (Vector3.SqrMagnitude(_unit.transform.position - targetPos) < 0.0025f)
        {
            AdvancePhase();
        }

        return false;
    }

    private void AdvancePhase()
    {
        if (_phase == MovePhase.ToEdge)
        {
            HandleNodeExit(_currentNode);
            _currentNode = _nextNode;
            HandleNodeEnter(_currentNode);
            _phase = MovePhase.ToStance;
        }
        else if (_phase == MovePhase.ToStance)
        {
            _phase = MovePhase.AtStance;
        }
    }

    private void HandleNodeExit(Node node)
    {
        if (node != null)
        {
            node.TileEffect?.OnUnitExit(_unit);
            node.UnitsOnNode.Remove(_unit);
        }
    }

    private void HandleNodeEnter(Node node)
    {
        if (node != null)
        {
            node.TileEffect?.OnUnitEnter(_unit);
            if (!node.UnitsOnNode.Contains(_unit)) node.UnitsOnNode.Add(_unit);
            CheckForBattle(node);
        }
    }

    private void CheckForBattle(Node node)
    {
        foreach (var other in node.UnitsOnNode)
        {
            if (other != _unit && other.MyTeam != _unit.MyTeam && !other.IsDead)
            {
                _unit.ChangeState(new UnitBattleState(_unit, other));
                other.ChangeState(new UnitBattleState(other, _unit));
                return;
            }
        }
    }

    public void SetAllyDestination(Node dest) => _allyDestination = dest;
}
