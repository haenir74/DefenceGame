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
        // Debug.Log($"{unit.name} Entered Idle State");
    }

    public void Execute()
    {
        // Idle 상태에서도 적이 오는지 감시
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
        // Debug.Log($"{unit.name} Entered Move State");
    }

    public void Execute()
    {
        // Attack Move: 이동 중 적 감지 시 공격 전환
        if (unit.DetectTarget(out Unit target))
        {
            unit.TargetUnit = target;
            unit.ChangeState(new UnitAttackState(unit));
            return;
        }

        // 실제 이동 로직 수행
        if (unit.MoveTick()) 
        {
            // 이동 완료
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
        // Debug.Log($"{unit.name} Entered Attack State");
    }

    public void Execute()
    {
        Unit target = unit.TargetUnit;

        // 타겟이 없거나 죽었으면 이동/대기로 복귀
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

        // 사거리를 벗어났는지 확인
        float dist = Vector3.Distance(unit.transform.position, target.transform.position);
        float range = unit.Data != null ? unit.Data.attackRange : 1f;

        if (dist > range)
        {
            // 사거리 밖이면 다시 이동 (추격 로직이 있다면 여기서 추격 상태로 전환 가능)
            unit.ChangeState(new UnitMoveState(unit));
            return;
        }

        // 공격 수행 (Unit 내부에서 쿨타임 체크)
        unit.TryAttack(target);
    }

    public void Exit()
    {
        // 공격 상태 종료 시 후딜레이 등을 처리할 수도 있음
    }
}
