using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private List<TileEffectSO> effects = new List<TileEffectSO>();
    
    public virtual void OnUnitEnter(Unit unit)
    {
        foreach (var effect in effects)
        {
            effect.ExecuteEnter(unit);
        }
    }

    public virtual void OnUnitExit(Unit unit)
    {
        foreach (var effect in effects)
        {
            effect.ExecuteExit(unit);
        }
    }
}