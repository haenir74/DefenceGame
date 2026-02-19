using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispatchManager : Singleton<DispatchManager>
{
    [Header("Player Stats")]
    [Tooltip("플레이어 고유 파견 효율 (기본 1.0 = 100%)")]
    public float playerDispatchMultiplier = 1.0f;

    public int CalculateDispatchBonus()
    {
        if (UnitManager.Instance == null) return 0;

        float totalBonus = 0f;
        List<Unit> allUnits = UnitManager.Instance.GetAllUnits();

        foreach (Unit unit in allUnits)
        {
            //if (unit.IsDead || !unit.IsDispatched) continue;
            TileDataSO tileData = unit.CurrentNode?.CurrentTileData;
            if (tileData == null || !tileData.IsDispatchTile) continue;

            float baseReward = tileData.baseDispatchReward;
            float unitEfficiency = unit.Data.dispatchEfficiency;
            float playerEfficiency = this.playerDispatchMultiplier;
            float tileEfficiency = tileData.EfficiencyMultiplier;
            float unitReward = baseReward * unitEfficiency * playerEfficiency * tileEfficiency;

            totalBonus += unitReward;
            
            Debug.Log($"[Dispatch] {unit.Data.Name}: {baseReward} * {unitEfficiency:P0} * {playerEfficiency:P0} = {unitReward}G");
        }

        return Mathf.RoundToInt(totalBonus);
    }

    public void AddPlayerEfficiency(float amount)
    {
        playerDispatchMultiplier += amount;
    }
}