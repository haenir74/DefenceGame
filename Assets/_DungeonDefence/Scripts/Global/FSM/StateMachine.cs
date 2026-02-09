using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : class
{
    private T owner;
    public BaseState<T> CurrentState { get; private set; }

    public StateMachine(T owner)
    {
        this.owner = owner;
    }

    public void ChangeState(BaseState<T> newState)
    {
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState?.OnEnter();
    }

    public void Update()
    {
        CurrentState?.OnUpdate();
    }

    public void PhysicsUpdate()
    {
        CurrentState?.OnPhysicsUpdate();
    }
}
