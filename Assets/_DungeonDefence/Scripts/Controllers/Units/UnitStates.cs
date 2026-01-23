using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitIdleState : IState
{
    private Unit unit;

    public UnitIdleState(Unit unit)
    {
        this.unit = unit;
    }

    public void Enter()
    {
        unit.SetStateEnum(UnitState.Idle);
    }

    public void Execute()
    {
        if (unit.DetectTarget(out Unit target))
        {
            unit.ChangeState(new UnitBattleState(unit, target));
            if (target.CurrentState is UnitIdleState || target.CurrentState is UnitMoveState)
            {
                target.ChangeState(new UnitBattleState(target, unit));
            }
            return;
        }

        if (unit.Data != null)
        {
            unit.DecideNextMove();
            if (unit.HasPathRemaining())
            {
                unit.ChangeState(new UnitMoveState(unit));
            }
        }
    }

    public void Exit()
    {
        
    }
}

public class UnitMoveState : IState
{
    private Unit unit;

    public UnitMoveState(Unit unit)
    {
        this.unit = unit;
    }

    public void Enter()
    {
        unit.SetStateEnum(UnitState.Moving);
    }

    public void Execute()
    {
        if (unit.MoveTick())
        {
            unit.ChangeState(new UnitIdleState(unit));
        }
    }

    public void Exit()
    {
        
    }
}

public class UnitBattleState : IState
{
    private Unit unit;
    private Unit target;

    public UnitBattleState(Unit unit, Unit target)
    {
        this.unit = unit;
        this.target = target;
    }

    public void Enter()
    {
        unit.SetStateEnum(UnitState.Attacking);
        unit.TargetUnit = target;
    }

    public void Execute()
    {
        if (target == null || target.IsDead)
        {
            unit.TargetUnit = null;
            unit.ChangeState(new UnitIdleState(unit));
            return;
        }
        unit.TryAttack(target);
    }

    public void Exit()
    {
        unit.TargetUnit = null;
    }
}
