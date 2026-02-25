using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 타일 오브젝트에 부착되어 효과 및 로직 담당
public class GridTile : MonoBehaviour
{
    [SerializeField] private TileDataSO data;
    public TileDataSO Data => data;

    public void Setup(TileDataSO data)
    {
        this.data = data;
    }

    // ─── 타일 이벤트 위임 ──────────────────────────────────────────────

    public void OnUnitEnter(Unit unit)
    {
        if (data?.tileEffect != null) data.tileEffect.OnEnter(unit);
    }

    public void OnUnitUpdate(Unit unit)
    {
        if (data?.tileEffect != null) data.tileEffect.OnUpdate(unit);
    }

    public void OnUnitExit(Unit unit)
    {
        if (data?.tileEffect != null) data.tileEffect.OnExit(unit);
    }

    public void OnUnitDeath(Unit unit)
    {
        if (data?.tileEffect != null) data.tileEffect.OnDeath(unit);
    }

    public void OnWaveClear(Unit unit)
    {
        if (data?.tileEffect != null) data.tileEffect.OnWaveClear(unit);
    }
}
