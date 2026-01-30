using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : class
{
    public T Owner { get; private set; }
    public BaseState<T> CurrentState { get; private set; }
    public BaseState<T> PreviousState { get; private set; }

    public StateMachine(T owner, BaseState<T> initialState = null)
    {
        Owner = owner;
        if (initialState != null)
        {
            ChangeState(initialState);
        }
    }

    public void ChangeState(BaseState<T> newState)
    {
        if (CurrentState == newState) return;

        CurrentState?.Exit();

        PreviousState = CurrentState;
        CurrentState = newState;

        if (CurrentState != null)
        {
            CurrentState.Initialize(Owner, this);
            CurrentState.Enter();
        }
    }

    public void Update()
    {
        CurrentState?.Update();
    }

    public void PhysicsUpdate()
    {
        CurrentState?.PhysicsUpdate();
    }

    public void RevertState()
    {
        if (PreviousState != null)
        {
            ChangeState(PreviousState);
        }
    }
}
