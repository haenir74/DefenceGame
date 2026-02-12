using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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

        iconImage.sprite = unitData.Icon;
        nameText.text = unitData.Name; 
        costText.text = $"{unitData.GetCosts()[0].amount} G";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }
    
    public void Initialize(TileDataSO tileData)
    {
        targetUnit = null;
        targetTile = tileData;

        iconImage.sprite = tileData.Icon;
        nameText.text = tileData.Name;
        costText.text = $"{tileData.GetCosts()[0].amount} G";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void OnBuyClicked()
    {
        OnBuyClick?.Invoke(slotIndex);
    }
}