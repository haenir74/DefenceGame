using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopUIView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ShopSlotUI slotPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button rerollButton;
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("Settings")]
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
        // 기존 슬롯 전체 제거
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (slotPrefab == null)
        {
            Debug.LogError("[ShopUIView] slotPrefab이 null입니다. Inspector에서 ShopSlot_Prefab을 연결해주세요.");
            return;
        }

        var units = ShopManager.Instance.GetUnitCatalog();
        var tiles = ShopManager.Instance.GetTileCatalog();

        int slotCount = 0;

        // 유닛 슬롯 생성 (아이템이 있는 경우에만)
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

        // 타일 슬롯 생성 (남은 슬롯 한도 내에서)
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
            Debug.Log("[ShopUIView] 상점에 표시할 아이템이 없습니다.");
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
