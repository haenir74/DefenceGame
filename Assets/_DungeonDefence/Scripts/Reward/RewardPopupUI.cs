using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RewardPopupUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI baseRewardText;
    [SerializeField] private TextMeshProUGUI bonusRewardText;
    [SerializeField] private TextMeshProUGUI totalRewardText;
    [SerializeField] private Button confirmButton;

    public event Action OnConfirm;

    private void Start()
    {
        confirmButton.onClick.AddListener(() => 
        {
            OnConfirm?.Invoke();
            Close();
        });
        gameObject.SetActive(false);
    }

    public void ShowReward(int wave, int baseGold, int bonusGold)
    {
        gameObject.SetActive(true);
        
        int totalGold = baseGold + bonusGold;

        if (titleText != null) titleText.text = $"WAVE {wave} CLEAR";
        if (baseRewardText != null) baseRewardText.text = $"기본 보상 : {baseGold} G";
        if (bonusRewardText != null) 
        {
            if (bonusGold > 0)
                bonusRewardText.text = $"파견 보너스 : +{bonusGold} G";
            else
                bonusRewardText.text = "파견 보너스 : 없음";
        }

        if (totalRewardText != null) totalRewardText.text = $"총 획득 : <color=yellow>{totalGold} G</color>";
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}