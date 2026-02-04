using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : class
{
    public T Owner { get; private set; }
    public BaseState<T> CurrentState { get; private set; }

    public StateMachine(T owner, BaseState<T> initialState = null)
    {
        Owner = owner;
        if (initialState != null)
        {
            ChangeState(initialState);
        }
    }

    public void Initialize(BaseState<T> startState)
    {
        CurrentState = startState;
        if (CurrentState != null)
        {
            CurrentState.OnEnter(Owner);
        }
    }

    public void ChangeState(BaseState<T> newState)
    {
        if (CurrentState != null) CurrentState.OnExit(Owner);
        CurrentState = newState;
        if (CurrentState != null) CurrentState.OnEnter(Owner);
    }

    public void Update()
    {
        CurrentState?.OnUpdate(Owner);
    }

    public void PhysicsUpdate()
    {
        CurrentState?.OnPhysicsUpdate(Owner);
    }
}
