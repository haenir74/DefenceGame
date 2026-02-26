using UnityEngine;

public abstract class TileEffectDataSO : ScriptableObject
{
    
    public string effectName;
    [TextArea] public string description;

    
    public virtual void OnEnter(Unit unit) { }

    
    public virtual void OnUpdate(Unit unit) { }

    
    public virtual void OnExit(Unit unit) { }

    
    public virtual void OnDeath(Unit unit) { }

    
    public virtual void OnWaveClear(Unit unit) { }
}



