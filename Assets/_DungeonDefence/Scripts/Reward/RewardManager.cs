using UnityEngine;

public class RewardManager : Singleton<RewardManager>
{
    [Header("Settings")]
    [SerializeField] private RewardPopupUI rewardPopup;
    [SerializeField] private int baseGoldPerWave = 100;
    [SerializeField] private int goldIncrementPerWave = 50;

    public void ProcessWaveClear(int clearedWaveIndex)
    {
        int baseGold = CalculateBaseReward(clearedWaveIndex);
        
        int dispatchBonus = 0;
        if (DispatchManager.Instance != null)
        {
            dispatchBonus = DispatchManager.Instance.CalculateDispatchBonus();
        }
        else
        {
            Debug.LogWarning("[RewardManager] DispatchManager가 존재하지 않습니다.");
        }

        int totalGold = baseGold + dispatchBonus;

        EconomyManager.Instance?.AddCurrency(CurrencyType.Gold, totalGold);
        Debug.Log($"[Reward] Wave {clearedWaveIndex} 보상 지급: 합계 {totalGold}G (기본 {baseGold} + 파견 {dispatchBonus})");
        
        if (rewardPopup != null)
        {
            rewardPopup.ShowReward(clearedWaveIndex, baseGold, dispatchBonus);
            
            rewardPopup.OnConfirm -= HandleRewardConfirm;
            rewardPopup.OnConfirm += HandleRewardConfirm;
        }
        else
        {
            Debug.LogError("[RewardManager] Reward Popup UI가 연결되지 않았습니다.");
            CompleteRewardPhase();
        }
    }

    private void HandleRewardConfirm()
    {
        if (rewardPopup != null)
        {
            rewardPopup.OnConfirm -= HandleRewardConfirm;
        }
        CompleteRewardPhase();
    }

    private int CalculateBaseReward(int wave)
    {
        return baseGoldPerWave + (wave * goldIncrementPerWave);
    }

    private void CompleteRewardPhase()
    {
        Debug.Log("[RewardManager] 보상 단계 종료 -> 정비 페이즈로 전환");
        GameManager.Instance.SwitchToMaintenancePhase();
    }
}