using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispatchManager : Singleton<DispatchManager>
{
    [Header("Settings")]
    [Tooltip("기본 파견 보상 (기본 100G)")]
    public int baseDispatchReward = 100;
    
    [Header("Player Stats")]
    [Tooltip("플레이어 고유 파견 효율 (기본 1.0 = 100%)")]
    public float playerDispatchMultiplier = 1.0f;

    /// <summary>
    /// 특정 유닛이나 데이터의 파견 보너스를 계산합니다.
    /// </summary>
    public int CalculateUnitBonus(Unit unit = null, UnitDataSO data = null)
    {
        UnitDataSO sourceData = unit != null ? unit.Data : data;
        if (sourceData == null) return 0;

        float efficiency = sourceData.dispatchEfficiency;
        float reward = baseDispatchReward * efficiency * playerDispatchMultiplier;

        return Mathf.RoundToInt(reward);
    }

    /// <summary>
    /// 현재 파견 패널에 등록된 모든 유닛의 총 보너스를 계산합니다.
    /// RewardManager에서 호출됩니다.
    /// </summary>
    public int CalculateTotalBonus()
    {
        if (DispatchPanelUI.Instance == null) return 0;
        return DispatchPanelUI.Instance.GetTotalDispatchBonus();
    }

    public void AddPlayerEfficiency(float amount)
    {
        playerDispatchMultiplier += amount;
    }
}
