using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopUIView : MonoBehaviour
{
    
    [SerializeField] private Transform contentParent;
    [SerializeField] private ShopSlotUI slotPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button rerollButton;
    [SerializeField] private TextMeshProUGUI goldText;

    
    [SerializeField] private int maxSlots = 8;

    public event Action OnCloseRequested;
    public event Action OnRerollRequested;

    private void Awake()
    {
        if (closeButton) closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
        if (rerollButton) rerollButton.onClick.AddListener(() => OnRerollRequested?.Invoke());
    }

    public void Open() => gameObject.SetActive(true);
    public void Close() => gameObject.SetActive(false);

    public void UpdateGoldText(int currentGold)
    {
        if (goldText != null) goldText.text = $"Gold: {currentGold:N0}";
    }

    public void RefreshShop()
    {
        
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (slotPrefab == null)
        {
            
            return;
        }

        var units = ShopManager.Instance.GetUnitCatalog();
        var tiles = ShopManager.Instance.GetTileCatalog();

        int slotCount = 0;

        
        if (units != null)
        {
            foreach (var unit in units)
            {
                if (slotCount >= maxSlots) break;
                if (unit == null) continue;
                CreateSlot(unit);
                slotCount++;
            }
        }

        
        if (tiles != null)
        {
            foreach (var tile in tiles)
            {
                if (slotCount >= maxSlots) break;
                if (tile == null) continue;
                CreateSlot(tile);
                slotCount++;
            }
        }

        if (slotCount == 0)
        {
            
        }
    }

    private void CreateSlot(ITradable item)
    {
        ShopSlotUI slot = Instantiate(slotPrefab, contentParent);
        if (item is UnitDataSO unit) slot.Initialize(unit);
        else if (item is TileDataSO tile) slot.Initialize(tile);
        slot.OnBuyClick += (_) => ShopManager.Instance.TryBuyItem(item);
    }
}



