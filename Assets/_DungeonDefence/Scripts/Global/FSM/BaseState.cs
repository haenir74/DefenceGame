using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState<T> where T : class
{
    protected T Controller { get; private set; }

    public BaseState(T controller)
    {
        this.Controller = controller;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnPhysicsUpdate() { }
    public virtual void OnExit() { }
}
