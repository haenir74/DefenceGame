using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispatchManager : Singleton<DispatchManager>
{
    
    
    public int baseDispatchReward = 100;
    
    
    
    public float playerDispatchMultiplier = 1.0f;

    private void Start()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveCompleted += HandleWaveCompleted;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveCompleted -= HandleWaveCompleted;
        }
    }

    private void HandleWaveCompleted()
    {
        int bonus = CalculateTotalBonus();
        if (bonus > 0 && EconomyManager.InstanceExists)
        {
            EconomyManager.Instance.AddCurrency(CurrencyType.Gold, bonus);
        }
    }

    
    
    
    public int CalculateUnitBonus(Unit unit = null, UnitDataSO data = null)
    {
        UnitDataSO sourceData = unit != null ? unit.Data : data;
        if (sourceData == null) return 0;

        float efficiency = sourceData.dispatchEfficiency;
        float reward = baseDispatchReward * efficiency * playerDispatchMultiplier;

        return Mathf.RoundToInt(reward);
    }

    
    
    
    
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



