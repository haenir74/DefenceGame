using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileEffectSO : ScriptableObject
{
    public abstract void ExecuteEnter(Unit unit);
    
    public abstract void ExecuteExit(Unit unit);
}