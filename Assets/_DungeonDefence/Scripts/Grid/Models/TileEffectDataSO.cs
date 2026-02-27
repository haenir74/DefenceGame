using UnityEngine;

using System;

public abstract class TileEffectDataSO : ScriptableObject, ITileEffectHandler
{
    public string effectName;
    [TextArea] public string description;


    [Obsolete("ExecuteEnterEffect를 대신 사용하십시오.")]
    public virtual void OnEnter(Unit unit) { }
    [Obsolete("ExecuteUpdateEffect를 대신 사용하십시오.")]
    public virtual void OnUpdate(Unit unit) { }
    [Obsolete("ExecuteExitEffect를 대신 사용하십시오.")]
    public virtual void OnExit(Unit unit) { }
    [Obsolete("ExecuteDeathEffect를 대신 사용하십시오.")]
    public virtual void OnDeath(Unit unit) { }
    [Obsolete("ExecuteWaveClearEffect를 대신 사용하십시오.")]
    public virtual void OnWaveClear(Unit unit) { }

    public virtual void ExecuteEnterEffect(Unit targetUnit) { OnEnter(targetUnit); }
    public virtual void ExecuteUpdateEffect(Unit targetUnit) { OnUpdate(targetUnit); }
    public virtual void ExecuteExitEffect(Unit targetUnit) { OnExit(targetUnit); }
    public virtual void ExecuteDeathEffect(Unit targetUnit) { OnDeath(targetUnit); }
    public virtual void ExecuteWaveClearEffect(Unit targetUnit) { OnWaveClear(targetUnit); }
}




