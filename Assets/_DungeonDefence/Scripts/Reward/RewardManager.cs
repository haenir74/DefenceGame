using UnityEngine;

public class RewardManager : Singleton<RewardManager>
{
    [Header("Settings")]
    [SerializeField] private RewardPopupUI rewardPopup;
    [SerializeField] private int baseGoldPerWave = 100;
    [SerializeField] private int goldIncrementPerWave = 50;

    private float GetCurrentDispatchEfficiency()
    {
        // 추후 DispatchManager.Instance.GetEfficiency()로 대체
        return 1.0f; 
    }

    public void ProcessWaveClear(int clearedWaveIndex)
    {
        int baseGold = CalculateBaseReward(clearedWaveIndex);
        float efficiency = GetCurrentDispatchEfficiency();
        
        int bonusGold = Mathf.RoundToInt(baseGold * (efficiency - 1.0f));
        int totalGold = baseGold + bonusGold;

        EconomyManager.Instance?.AddCurrency(CurrencyType.Gold, totalGold);
        Debug.Log($"[Reward] Wave {clearedWaveIndex} 보상 지급: {totalGold}G (기본 {baseGold} + 보너스 {bonusGold})");

        if (rewardPopup != null)
        {
            rewardPopup.ShowReward(clearedWaveIndex, baseGold, bonusGold);
            
            rewardPopup.OnConfirm -= () => 
            {
                CompleteRewardPhase();
            };
            rewardPopup.OnConfirm += () => 
            {
                CompleteRewardPhase();
            };
        }
        else
        {
            CompleteRewardPhase();
        }
    }

    private int CalculateBaseReward(int wave)
    {
        return baseGoldPerWave + (wave * goldIncrementPerWave);
    }

    private void CompleteRewardPhase()
    {
        GameManager.Instance.SwitchToMaintenancePhase();
    }
}