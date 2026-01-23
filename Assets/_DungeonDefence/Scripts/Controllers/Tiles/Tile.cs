using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private List<TileEffectSO> effects = new List<TileEffectSO>();
    
    private int? _cachedAttractiveness = null;

    public void InvalidateCache()
    {
        _cachedAttractiveness = null;
    }

    public virtual void OnUnitEnter(Unit unit)
    {
        if (effects == null) return;
        foreach (var effect in effects)
        {
            if (effect != null) effect.ExecuteEnter(unit);
        }
    }

    public virtual void OnUnitExit(Unit unit)
    {
        if (effects == null) return;
        foreach (var effect in effects)
        {
            if (effect != null) effect.ExecuteExit(unit);
        }
    }

    public int GetTotalAttractiveness()
    {
        if (_cachedAttractiveness.HasValue)
        {
            return _cachedAttractiveness.Value;
        }

        int total = 0;
        foreach (var effect in effects)
        {
            if (effect != null) total += effect.Attractiveness;
        }
        _cachedAttractiveness = total;
        return total;
    }

    private void OnValidate()
    {
        _cachedAttractiveness = null;
    }
}