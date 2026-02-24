using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ShopSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject soldOutCover;
    
    private UnitDataSO targetUnit;
    private TileDataSO targetTile;
    private int slotIndex;
    private bool isSoldOut = false;

    public event Action<int> OnBuyClick;

    public void Initialize(UnitDataSO unitData)
    {
        targetUnit = unitData;
        targetTile = null;

        if (iconImage != null) iconImage.sprite = unitData.Icon;
        if (nameText != null) nameText.text = unitData.Name;
        if (costText != null) costText.text = GetCostText(unitData.GetCosts());

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }
    
    public void Initialize(TileDataSO tileData)
    {
        targetUnit = null;
        targetTile = tileData;

        if (iconImage != null) iconImage.sprite = tileData.Icon;
        if (nameText != null) nameText.text = tileData.Name;
        if (costText != null) costText.text = GetCostText(tileData.GetCosts());

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    private string GetCostText(List<ResourceCost> costs)
    {
        if (costs == null || costs.Count == 0) return "무료";
        return $"{costs[0].amount} G";
    }

    private void OnBuyClicked()
    {
        OnBuyClick?.Invoke(slotIndex);
    }
}
