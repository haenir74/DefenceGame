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

    public void Setup(PerkDataSO data)
    {
        this.perkData = data;
        UpdateVisuals();
    }

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
            if (MetaManager.Instance.TryUnlockPerk(perkData))
            {
                UpdateVisuals();
                PerkTreeManager.Instance?.RefreshAllNodes();
            }
        }
    }

    public void UpdateVisuals()
    {
        if (perkData == null || MetaManager.Instance == null) return;

        bool isUnlocked = MetaManager.Instance.IsPerkUnlocked(perkData.PerkID);
        bool isAvailable = IsAvailable();

        if (activeIconObj != null) activeIconObj.SetActive(isUnlocked);
        if (inactiveIconObj != null) inactiveIconObj.SetActive(!isUnlocked);

        if (upgradeButton != null)
        {
            upgradeButton.interactable = !isUnlocked && isAvailable && MetaManager.Instance.PerkPoints >= perkData.Cost;
        }

        if (perkNameText != null) perkNameText.text = perkData.DisplayName;
        if (descriptionText != null) descriptionText.text = perkData.Description;
        if (costText != null) costText.text = isUnlocked ? "Unlocked" : $"{perkData.Cost} SP";
    }

    public void ToggleTooltip(bool show)
    {
        if (tooltipPopup != null) tooltipPopup.SetActive(show);
        if (show) UpdateVisuals();
    }

    private bool IsAvailable()
    {
        if (perkData == null) return true;
        if (perkData.Prerequisites == null || perkData.Prerequisites.Length == 0)
            return true;

        foreach (var preData in perkData.Prerequisites)
        {
            if (preData != null && !MetaManager.Instance.IsPerkUnlocked(preData.PerkID))
                return false;
        }
        return true;
    }

    public Vector2 GetPosition()
    {
        return GetComponent<RectTransform>().anchoredPosition;
    }
}



