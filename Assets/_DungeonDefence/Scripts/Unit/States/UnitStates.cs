using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitState : BaseState<Unit>
{
    protected Unit Self => Controller;
    protected UnitState(Unit unit) : base(unit) {}

    public virtual void OnStepFinished() { }
}

public class UnitIdleState : UnitState
{
    public UnitIdleState(Unit unit) : base(unit) {}

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

        Unit target = UnitManager.Instance.GetOpponentAt(Self.Coordinate, Self.IsPlayerTeam);
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
        Unit target = UnitManager.Instance.GetOpponentAt(Self.Coordinate, Self.IsPlayerTeam);
        if (target != null)
        {
            Self.StartCombat();
            return;
        }
        if (Self.PathFinder != null)
        {
            Vector2Int? nextStep = Self.PathFinder.GetTargetStep();
            
            if (nextStep.HasValue)
            {
                Self.Movement.MoveTo(nextStep.Value);
            }
            else
            {
                Self.PathFinder.FindNextStep();
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
        this.target = UnitManager.Instance.GetOpponentAt(Self.Coordinate, Self.IsPlayerTeam);
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
        UnitManager.Instance.AttackUnit(Self, this.target);
    }

    public override void OnExit() 
    {
        target = null;
    }
}