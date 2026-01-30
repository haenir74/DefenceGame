using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitIdleState : BaseState<Unit>
{
    public override void Enter()
    {
        Debug.Log("Unit Idle");
    }

    public override void Update()
    {
        
    }
}

public class UnitMoveState : BaseState<Unit>
{
    private Unit _target;
    private GridNode _destNode;

    public UnitMoveState(Unit target)
    {
        _target = target;
    }
    
    public UnitMoveState(GridNode destination)
    {
        _destNode = destination;
    }

    public override void Enter()
    {
        if (_target != null)
        {
            
        }
        else if (_destNode != null)
        {
             Controller.Movement.MoveTo(_destNode, OnMoveComplete);
        }
    }

    private void OnMoveComplete()
    {
        Controller.FSM.ChangeState(new UnitIdleState());
    }
}

public class UnitAttackState : BaseState<Unit>
{
    private Unit _target;
    
    public UnitAttackState(Unit target) { _target = target; }

    public override void Update()
    {
        if (_target == null || _target.Combat.IsDead)
        {
            Controller.FSM.ChangeState(new UnitIdleState());
            return;
        }

        bool attacked = Controller.Combat.TryAttack(_target);
    }
}