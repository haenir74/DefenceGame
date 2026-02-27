using UnityEngine;

using System;

public abstract class TileEffectDataSO : ScriptableObject
{
    public string effectName;
    [TextArea] public string description;

    [Header("Stack Rules")]
    public float baseDuration = 5f;
    public bool isStackable = false;
    public int maxStacks = 1;

    public abstract void ApplyEffect(Unit target, int currentStacks);
    public abstract void RemoveEffect(Unit target, int currentStacks);

    public virtual void ExecuteDeathEffect(Unit target, int currentStacks) { }
}




