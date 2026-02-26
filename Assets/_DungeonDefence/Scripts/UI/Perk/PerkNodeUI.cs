using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PerkNodeUI : MonoBehaviour
{
    
    [SerializeField] private GameObject activeIconObj;
    [SerializeField] private GameObject inactiveIconObj;

    
    [SerializeField] private GameObject tooltipPopup;
    [SerializeField] private TextMeshProUGUI perkNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;

    
    [SerializeField] private Button upgradeButton;
    [SerializeField] private PerkDataSO perkData;

    public PerkDataSO Data => perkData;

    private void Start()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

        UpdateVisuals();
    }

    private void OnUpgradeClicked()
    {
        if (perkData != null && MetaManager.Instance != null)
        {
            MetaManager.Instance.UpgradePerk(perkData.perkId);
            UpdateVisuals();
            PerkTreeManager.Instance?.RefreshAllNodes();
        }
    }

    public void UpdateVisuals()
    {
        if (perkData == null || MetaManager.Instance == null) return;

        int level = MetaManager.Instance.GetPerkLevel(perkData.perkId);
        bool isMaxed = level >= perkData.maxLevel;
        bool isUnlocked = IsUnlocked();

        if (activeIconObj != null) activeIconObj.SetActive(isUnlocked);
        if (inactiveIconObj != null) inactiveIconObj.SetActive(!isUnlocked);

        if (upgradeButton != null)
            upgradeButton.interactable = isUnlocked && !isMaxed && MetaManager.Instance.PerkPoints >= perkData.GetCost(level);

        if (perkNameText != null) perkNameText.text = perkData.perkName;
        if (descriptionText != null) descriptionText.text = perkData.description;
        if (costText != null) costText.text = isMaxed ? "MAX" : $"{perkData.GetCost(level)} SP";
    }

    public void ToggleTooltip(bool show)
    {
        if (tooltipPopup != null) tooltipPopup.SetActive(show);
        if (show) UpdateVisuals();
    }

    private bool IsUnlocked()
    {
        if (perkData == null) return true;
        if (perkData.prerequisitePerks == null || perkData.prerequisitePerks.Count == 0)
            return true;

        foreach (string preId in perkData.prerequisitePerks)
        {
            if (MetaManager.Instance.GetPerkLevel(preId) <= 0)
                return false;
        }
        return true;
    }

    public Vector2 GetPosition()
    {
        return GetComponent<RectTransform>().anchoredPosition;
    }
}



