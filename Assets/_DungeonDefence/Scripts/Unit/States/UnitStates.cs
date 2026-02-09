using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.UI;

public abstract class UnitState : BaseState<Unit>
{
    protected Unit Self => Controller;
    protected UnitState(Unit unit) : base(unit) {}
}

public class UnitIdleState : UnitState
{
    public UnitIdleState(Unit unit) : base(unit) {}

    public override void OnEnter() { }

    public override void OnUpdate()
    {
        if (Self.IsDead) return;
        CheckCombatCondition();
    }

    public override void OnExit() { }

    private void CheckCombatCondition()
    {
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
        if (Self.IsDead) return;
        if (CheckCombatCondition()) return;
    }

    public override void OnExit() { }

    private bool CheckCombatCondition()
    {
        Unit target = UnitManager.Instance.GetOpponentAt(Self.Coordinate, Self.IsPlayerTeam);
        
        if (target != null)
        {
            Self.StartCombat();
            return true;
        }
        return false;
    }
}

public class UnitCombatState : UnitState
{
    private Unit target;

    public UnitCombatState(Unit unit) : base(unit) { }

    public override void OnEnter()
    {
        this.target = UnitManager.Instance.GetOpponentAt(Self.Coordinate, Self.IsPlayerTeam);

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