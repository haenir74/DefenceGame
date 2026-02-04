using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitIdleState : BaseState<Unit>
{
    public override void OnEnter(Unit unit) { }

    public override void OnUpdate(Unit unit)
    {
        CheckCombatCondition(unit);
    }

    public override void OnExit(Unit unit) { }

    private void CheckCombatCondition(Unit unit)
    {
        Unit target = UnitManager.Instance.GetOpponentAt(unit.Coordinate, unit.IsPlayerTeam);
        
        if (target != null)
        {
            unit.StartCombat();
        }
    }
}


public class EnemyTurnState : BaseState<Unit>
{
    public override void OnEnter(Unit unit) { }

    public override void OnUpdate(Unit unit)
    {
        if (CheckCombatCondition(unit)) return;
    }

    public override void OnExit(Unit unit) { }

    private bool CheckCombatCondition(Unit unit)
    {
        Unit target = UnitManager.Instance.GetOpponentAt(unit.Coordinate, unit.IsPlayerTeam);
        
        if (target != null)
        {
            unit.StartCombat();
            return true;
        }
        return false;
    }
}

public class UnitCombatState : BaseState<Unit>
{
    private Unit target;

    public override void OnEnter(Unit unit)
    {
        this.target = UnitManager.Instance.GetOpponentAt(unit.Coordinate, unit.IsPlayerTeam);

        if (this.target == null)
        {
            unit.EndCombat();
        }
    }

    public override void OnUpdate(Unit unit)
    {
        if (target == null || target.IsDead || target.Coordinate != unit.Coordinate)
        {
            unit.EndCombat();
            return;
        }
        UnitManager.Instance.AttackUnit(unit, this.target);
    }

    public override void OnExit(Unit unit) { }
}