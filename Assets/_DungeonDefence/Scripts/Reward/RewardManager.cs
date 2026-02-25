using UnityEngine;

public class RewardManager : Singleton<RewardManager>
{
    [Header("Settings")]
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
            StartUnlockPhase();
        }
    }

    private void HandleRewardConfirm()
    {
        if (rewardPopup != null)
            rewardPopup.OnConfirm -= HandleRewardConfirm;

        StartUnlockPhase();
    }

    private void StartUnlockPhase()
    {
        if (unlockPopup != null && ShopManager.Instance != null)
        {
            var candidates = ShopManager.Instance.GetUnlockCandidates(3);
            if (candidates.Count > 0)
            {
                unlockPopup.OnUnlockConfirmed -= CompleteRewardPhase;
                unlockPopup.OnUnlockConfirmed += CompleteRewardPhase;
                unlockPopup.Show(candidates);
                return;
            }
        }
        // 해금 팝업 없거나 후보 없으면 바로 완료
        CompleteRewardPhase();
    }

    private void CompleteRewardPhase()
    {
        if (unlockPopup != null)
            unlockPopup.OnUnlockConfirmed -= CompleteRewardPhase;

        // 웨이브마다 재고 초기화
        ShopManager.Instance?.ResetStocksForNewWave();

        // 다음 웨이브 인덱스로 티어 확률 가져와 상점 재구성
        TierProbabilities tierProbs = GetNextWaveTierProbs();
        ShopManager.Instance?.RollShopItems(tierProbs);

        Debug.Log("[RewardManager] 보상 단계 종료 -> 정비 페이즈로 전환");
        GameManager.Instance.SwitchToMaintenancePhase();
    }

    private TierProbabilities GetNextWaveTierProbs()
    {
        // 다음 웨이브(= CurrentWave+1)의 WaveData에서 티어 확률 읽기
        // WaveManager가 없거나 해당 Wave 데이터가 없으면 null 반환 (단순 랜덤 폴백)
        if (WaveManager.Instance == null) return null;
        return WaveManager.Instance.GetNextWaveTierProbs();
    }

    private int CalculateBaseReward(int wave)
    {
        return baseGoldPerWave + (wave * goldIncrementPerWave);
    }
}
