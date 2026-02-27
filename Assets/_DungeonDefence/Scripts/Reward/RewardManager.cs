using UnityEngine;

public class RewardManager : Singleton<RewardManager>
{

    [SerializeField] private RewardPopupUI rewardPopup;
    [SerializeField] private ShopUnlockPopupUI unlockPopup;
    [SerializeField] private int baseGoldPerWave = 100;
    [SerializeField] private int goldIncrementPerWave = 50;

    public void ProcessWaveClear(int clearedWaveIndex)
    {
        int baseGold = CalculateBaseReward(clearedWaveIndex);

        int dispatchBonus = 0;
        if (DispatchManager.Instance != null)
        {
            dispatchBonus = DispatchManager.Instance.CalculateTotalBonus();
        }

        else
        {

        }

        int totalGold = baseGold + dispatchBonus;

        EconomyManager.Instance?.AddCurrency(CurrencyType.Gold, totalGold);


        if (rewardPopup != null)
        {
            rewardPopup.ShowReward(clearedWaveIndex, baseGold, dispatchBonus);

            rewardPopup.OnConfirm -= HandleRewardConfirm;
            rewardPopup.OnConfirm += HandleRewardConfirm;
        }
        else
        {
            CompleteRewardPhase();
        }
    }

    private void HandleRewardConfirm()
    {
        if (rewardPopup != null)
            rewardPopup.OnConfirm -= HandleRewardConfirm;

        CompleteRewardPhase();
    }

    private void CompleteRewardPhase()
    {
        if (unlockPopup != null)
            unlockPopup.OnUnlockConfirmed -= CompleteRewardPhase;

        ShopManager.Instance?.ResetStocksForNewWave();

        TierProbabilities tierProbs = GetNextWaveTierProbs();
        ShopManager.Instance?.RollShopItems(tierProbs);


        GameManager.Instance.SwitchToMaintenancePhase();
    }

    private TierProbabilities GetNextWaveTierProbs()
    {

        if (WaveManager.Instance == null) return null;
        return WaveManager.Instance.GetNextWaveTierProbs();
    }

    private int CalculateBaseReward(int wave)
    {
        return baseGoldPerWave + (wave * goldIncrementPerWave);
    }
}



