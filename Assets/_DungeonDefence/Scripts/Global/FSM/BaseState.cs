using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState<T> where T : class
{
    protected T Controller { get; private set; }
    protected StateMachine<T> Machine { get; private set; }

    public void Initialize(T controller, StateMachine<T> machine)
    {
        Controller = controller;
        Machine = machine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void PhysicsUpdate() { }
    public virtual void Exit() { }
}
