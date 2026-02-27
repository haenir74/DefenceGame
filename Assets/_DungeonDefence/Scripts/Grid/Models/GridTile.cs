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
        if (data?.tileEffect is ITileEffectHandler effectHandler) effectHandler.ExecuteEnterEffect(unit);
    }

    public void OnUnitUpdate(Unit unit)
    {
        if (data?.tileEffect is ITileEffectHandler effectHandler) effectHandler.ExecuteUpdateEffect(unit);
    }

    public void OnUnitExit(Unit unit)
    {
        if (data?.tileEffect is ITileEffectHandler effectHandler) effectHandler.ExecuteExitEffect(unit);
    }

    public void OnUnitDeath(Unit unit)
    {
        if (data?.tileEffect is ITileEffectHandler effectHandler) effectHandler.ExecuteDeathEffect(unit);
    }

    public void OnWaveClear(Unit unit)
    {
        if (data?.tileEffect is ITileEffectHandler effectHandler) effectHandler.ExecuteWaveClearEffect(unit);
    }
}


