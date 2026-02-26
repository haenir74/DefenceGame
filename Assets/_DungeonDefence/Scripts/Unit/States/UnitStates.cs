using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitState : BaseState<Unit>
{
    protected Unit Self => Controller;
    protected UnitState(Unit unit) : base(unit) { }

    public virtual void OnStepFinished() { }
}

public class UnitIdleState : UnitState
{
    public UnitIdleState(Unit unit) : base(unit) { }

    public override void OnEnter()
    {
        CheckCombat();
    }

    public override void OnUpdate()
    {
        CheckCombat();
    }

    public override void OnExit() { }

    private void CheckCombat()
    {
        if (Self.IsDead) return;
        if (Self.IsDispatched) return;

        Unit target = UnitManager.Instance.GetRandomOpponentAt(Self.Coordinate, Self.IsPlayerTeam);
        if (target != null)
        {
            Self.StartCombat();
        }
    }
}

public class EnemyTurnState : UnitState
{
    public EnemyTurnState(Unit unit) : base(unit) { }

    public override void OnEnter() { }

    public override void OnUpdate()
    {
        Act();
    }

    public override void OnStepFinished()
    {
        Act();
    }

    public override void OnExit() { }

    private void Act()
    {
        if (Self.IsDead) return;
        if (Self.Movement.IsMoving) return;
        Unit target = UnitManager.Instance.GetRandomOpponentAt(Self.Coordinate, Self.IsPlayerTeam);
        if (target != null)
        {
            Self.StartCombat();
            return;
        }

        List<GridNode> neighbors = GridManager.Instance.GetNeighbors(Self.CurrentNode);
        if (neighbors != null && neighbors.Count > 0)
        {
            List<GridNode> bestNodes = new List<GridNode>();
            float maxScore = float.MinValue;

            
            int penaltyPerVisit = 50;
            float noiseRange = 5f;

            foreach (var n in neighbors)
            {
                
                float score = n.Attractiveness;

                
                int visitCount = 0;
                foreach (var visitedCoord in Self.VisitedHistory)
                {
                    if (visitedCoord == n.Coordinate) visitCount++;
                }
                score -= visitCount * penaltyPerVisit;

                
                score += Random.Range(-noiseRange, noiseRange);

                if (score > maxScore)
                {
                    maxScore = score;
                    bestNodes.Clear();
                    bestNodes.Add(n);
                }
                else if (Mathf.Approximately(score, maxScore))
                {
                    bestNodes.Add(n);
                }
            }

            if (bestNodes.Count > 0)
            {
                GridNode bestNode = bestNodes[Random.Range(0, bestNodes.Count)];
                Self.Movement.MoveTo(bestNode.Coordinate);
            }
        }
    }
}

public class UnitCombatState : UnitState
{
    private Unit target;

    public UnitCombatState(Unit unit) : base(unit) { }

    public override void OnEnter()
    {
        this.target = UnitManager.Instance.GetRandomOpponentAt(Self.Coordinate, Self.IsPlayerTeam);
        if (Self.IsDispatched)
        {
            Self.EndCombat();
            return;
        }
        if (this.target == null)
        {
            Self.EndCombat();
        }
    }

    public override void OnUpdate()
    {
        if (target == null || target.IsDead || target.Coordinate != Self.Coordinate)
        {
            Self.EndCombat();
            return;
        }
        
        Unit randomTarget = UnitManager.Instance.GetRandomOpponentAt(Self.Coordinate, Self.IsPlayerTeam);
        if (randomTarget != null)
            UnitManager.Instance.AttackUnit(Self, randomTarget);
        else
            Self.EndCombat();
    }

    public override void OnExit()
    {
        target = null;
    }
}


