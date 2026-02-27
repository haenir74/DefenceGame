using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    [SerializeField] private TileDataSO data;
    public TileDataSO Data => data;

    public void Setup(TileDataSO data)
    {
        this.data = data;
    }

    public void OnUnitEnter(Unit unit)
    {
        if (data != null && data.tileEffect != null)
        {
            var handler = unit.GetComponent<StatusEffectHandler>();
            if (handler == null) handler = unit.gameObject.AddComponent<StatusEffectHandler>();
            handler.AddEffect(data.tileEffect as TileEffectDataSO);
        }
    }

    public void OnUnitUpdate(Unit unit)
    {

    }

    public void OnUnitExit(Unit unit)
    {

    }

    public void OnUnitDeath(Unit unit)
    {
        if (data != null && data.tileEffect != null)
        {
            var handler = unit.GetComponent<StatusEffectHandler>();
            if (handler != null) handler.RemoveEffect(data.tileEffect as TileEffectDataSO);
        }
    }

    public void OnWaveClear(Unit unit)
    {
        if (data != null && data.tileEffect != null)
        {
            var handler = unit.GetComponent<StatusEffectHandler>();
            if (handler != null) handler.RemoveEffect(data.tileEffect as TileEffectDataSO);
        }
    }
}


