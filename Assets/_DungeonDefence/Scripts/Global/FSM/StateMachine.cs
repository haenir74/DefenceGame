using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public IState CurrentState { get; private set; }
    public IState PreviousState { get; private set; }

    public bool DebugMode { get; set; } = false;
    private string ownerName;

    public StateMachine(string ownerName = "Unknown")
    {
        this.ownerName = ownerName;
    }

    public void ChangeState(IState newState)
    {
        if (CurrentState != null)
        {
            if (DebugMode) Debug.Log($"[{ownerName}] Exit: {CurrentState.GetType().Name}");
            CurrentState.Exit();
            PreviousState = CurrentState;
        }

        CurrentState = newState;

        if (CurrentState != null)
        {
            if (DebugMode) Debug.Log($"[{ownerName}] Enter: {CurrentState.GetType().Name}");
            CurrentState.Enter();
        }
    }

    public void Update()
    {
        CurrentState?.Execute();
    }
}
