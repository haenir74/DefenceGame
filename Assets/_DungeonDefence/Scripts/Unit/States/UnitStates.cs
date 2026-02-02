using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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


public class EnemyTurnState : BaseState<Unit>
{
    private EnemyPathfinder _pathfinder;

    public override void Initialize(Unit controller, StateMachine<Unit> machine)
    {
        base.Initialize(controller, machine);
        _pathfinder = controller.GetComponent<EnemyPathfinder>();
        if (_pathfinder == null) 
        {
            _pathfinder = controller.gameObject.AddComponent<EnemyPathfinder>();
            _pathfinder.Initialize(controller);
        }
    }

    public override void Enter()
    {
        GridNode nextNode = _pathfinder.GetNextNode();

        if (nextNode != null)
        {
            Controller.Movement.MoveTo(nextNode, () => OnMoveComplete(nextNode));
        }
        else
        {
            Controller.FSM.ChangeState(new UnitIdleState());
        }
    }

    private void OnMoveComplete(GridNode arrivedNode)
    {
        _pathfinder.RecordVisit(arrivedNode);
        
        if (Machine.CurrentState == this)
        {
            Controller.FSM.ChangeState(new EnemyTurnState());
        }
    }
}

public class UnitCombatState : BaseState<Unit>
{
    private Unit _target;
    private float _lastAttackTime;

    public override void Enter()
    {
        _lastAttackTime = Time.time;
        FindTarget();
    }

    public override void Update()
    {
        if (_target == null || _target.Combat.IsDead)
        {
            FindTarget();
            return;
        }
        Controller.Combat.TryAttack(_target);
    }

    private void FindTarget()
    {
        GridNode currentNode = Controller.CurrentNode;
        if (currentNode == null) return;
        _target = currentNode.UnitsOnTile
            .FirstOrDefault(u => u != Controller && 
                                 u.IsPlayerTeam != Controller.IsPlayerTeam && 
                                 !u.Combat.IsDead);
    }
}