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
            unit.TargetUnit = target;
            unit.ChangeState(new UnitAttackState(unit));
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
        if (unit.DetectTarget(out Unit target))
        {
            unit.TargetUnit = target;
            unit.ChangeState(new UnitAttackState(unit));
            return;
        }

        if (unit.MoveTick()) 
        {
            // 이동 완료 시 OnPathComplete 호출
            unit.TriggerPathComplete();
            unit.ChangeState(new UnitIdleState(unit));
        }
    }

    public void Exit()
    {
    }
}

public class UnitAttackState : IState
{
    private Unit unit;

    public UnitAttackState(Unit unit)
    {
        this.unit = unit;
    }

    public void Enter()
    {
        unit.SetStateEnum(UnitState.Attacking);
    }

    public void Execute()
    {
        Unit target = unit.TargetUnit;

        if (target == null || target.IsDead)
        {
            unit.TargetUnit = null;
            if (unit.HasPathRemaining())
            {
                unit.ChangeState(new UnitMoveState(unit));
            }
            else
            {
                unit.ChangeState(new UnitIdleState(unit));
            }
            return;
        }

        float dist = Vector3.Distance(unit.transform.position, target.transform.position);
        float range = unit.Data != null ? unit.Data.attackRange : 1f;

        if (dist > range)
        {
            unit.ChangeState(new UnitMoveState(unit));
            return;
        }

        unit.TryAttack(target);
    }

    public void Exit()
    {
    }
}