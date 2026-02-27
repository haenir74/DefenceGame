using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RewardPopupUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI baseRewardText;
    [SerializeField] private TextMeshProUGUI bonusRewardText;
    [SerializeField] private TextMeshProUGUI totalRewardText;
    [SerializeField] private Button confirmButton;

    public event Action OnConfirm;

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                OnConfirm?.Invoke();
                Close();
            });
        }
    }

    public void ShowReward(int wave, int baseGold, int bonusGold)
    {
        int totalGold = baseGold + bonusGold;

        if (titleText != null) titleText.text = $"WAVE {wave} CLEAR";
        if (baseRewardText != null) baseRewardText.text = $"기본 보상 : {baseGold} G";
        if (bonusRewardText != null)
        {
            bonusRewardText.text = bonusGold > 0 ? $"파견 보너스: +{bonusGold} G" : "파견 보너스: 없음";
        }
        if (totalRewardText != null) totalRewardText.text = $"총 획득 : <color=yellow>{totalGold} G</color>";
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}


