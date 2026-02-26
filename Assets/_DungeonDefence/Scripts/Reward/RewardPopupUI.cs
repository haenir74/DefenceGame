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
        if (baseRewardText != null) baseRewardText.text = $"湲곕낯 蹂댁긽 : {baseGold} G";
        if (bonusRewardText != null) 
        {
            bonusRewardText.text = bonusGold > 0 ? $"?뚭껄 蹂대꼫??: +{bonusGold} G" : "?뚭껄 蹂대꼫??: ?놁쓬";
        }
        if (totalRewardText != null) totalRewardText.text = $"珥??띾뱷 : <color=yellow>{totalGold} G</color>";
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}


