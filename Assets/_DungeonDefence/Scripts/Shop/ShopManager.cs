using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Panex.Inventory;

public class ShopManager : Singleton<ShopManager>
{
    [Header("Shop Settings")]
    [Tooltip("기본 상점 슬롯 수 (웨이브 진행에 따라 증가 가능)")]
    [SerializeField] private int activeSlotCount = 2;
    [SerializeField] private int rerollCost = 50;

    [Header("Unlock System")]
    [Tooltip("해금 후보 풀 SO. 웨이브 클리어 시 이 풀에서 선택지 제공.")]
    [SerializeField] private ShopUnlockPoolSO unlockPool;
    [Tooltip("게임 시작 시 기본으로 해금된 아이템 목록 (초기 상점에서 구매 가능)")]
    [SerializeField] private List<UnitDataSO> defaultUnlockedUnits = new List<UnitDataSO>();
    [SerializeField] private List<TileDataSO> defaultUnlockedTiles = new List<TileDataSO>();

    // ─── 런타임 상태 ───────────────────────────────────────────────────
    /// <summary>현재 해금된 전체 아이템 풀 (상점 랜덤 선택 대상)</summary>
    private List<ITradable> unlockedItems = new List<ITradable>();

    /// <summary>아이템별 남은 재고 (0 = 품절, -1 = 무한)</summary>
    private Dictionary<ITradable, int> stockRemaining = new Dictionary<ITradable, int>();

    /// <summary>현재 상점에 표시 중인 슬롯 아이템</summary>
    private List<ITradable> currentShopItems = new List<ITradable>();

    public event Action<string> OnPurchaseSuccess;
    public event Action OnPurchaseFailed;
    public event Action OnShopRefreshed;

    // ─── 초기화 ───────────────────────────────────────────────────────

    private void Start()
    {
        InitializeDefaultUnlocks();
        RollShopItems();
    }

    private void InitializeDefaultUnlocks()
    {
        foreach (var unit in defaultUnlockedUnits)
            if (unit != null) UnlockItem(unit);
        foreach (var tile in defaultUnlockedTiles)
            if (tile != null) UnlockItem(tile);
    }

    // ─── 해금 API ─────────────────────────────────────────────────────

    /// <summary>아이템을 해금 목록에 추가하고 재고를 등록.</summary>
    public void UnlockItem(ITradable item)
    {
        if (item == null || unlockedItems.Contains(item)) return;
        unlockedItems.Add(item);

        // 재고 설정
        int stock = GetInitialStock(item);
        stockRemaining[item] = stock <= 0 ? -1 : stock; // -1 = 무한
    }

    private int GetInitialStock(ITradable item)
    {
        if (item is UnitDataSO unit) return unit.shopStock;
        if (item is TileDataSO tile) return tile.shopStock;
        return -1;
    }

    /// <summary>웨이브 클리어 시 해금 후보 풀에서 count개 반환 (이미 해금된 것 제외).</summary>
    public List<ITradable> GetUnlockCandidates(int count = 3)
    {
        if (unlockPool == null) return new List<ITradable>();
        return unlockPool.GetRandomCandidates(count, unlockedItems);
    }

    // ─── 상점 슬롯 API ────────────────────────────────────────────────

    /// <summary>해금된 아이템 풀에서 activeSlotCount개를 무작위 선택해 상점 구성.</summary>
    public void RollShopItems()
    {
        currentShopItems.Clear();

        // 재고 있는 아이템만 후보
        List<ITradable> available = new List<ITradable>();
        foreach (var item in unlockedItems)
        {
            if (!stockRemaining.TryGetValue(item, out int stock)) continue;
            if (stock == -1 || stock > 0) available.Add(item);
        }

        // Fisher-Yates 셔플 후 슬롯 수만큼 선택
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        int take = Mathf.Min(activeSlotCount, available.Count);
        for (int i = 0; i < take; i++)
            currentShopItems.Add(available[i]);

        OnShopRefreshed?.Invoke();
    }

    public void RerollShop()
    {
        if (EconomyManager.Instance.CanAfford(new List<ResourceCost>
            { new ResourceCost { type = CurrencyType.Gold, amount = rerollCost } }))
        {
            EconomyManager.Instance.AddCurrency(CurrencyType.Gold, -rerollCost);
            RollShopItems();
        }
    }

    /// <summary>현재 상점에 표시되는 아이템 목록 (ShopUIView에서 호출).</summary>
    public List<ITradable> GetCurrentShopItems() => currentShopItems;

    /// <summary>[호환] 기존 코드에서 사용 중인 GetUnitCatalog 대체용.</summary>
    public List<UnitDataSO> GetUnitCatalog()
    {
        List<UnitDataSO> result = new List<UnitDataSO>();
        foreach (var item in currentShopItems)
            if (item is UnitDataSO u) result.Add(u);
        return result;
    }

    public List<TileDataSO> GetTileCatalog()
    {
        List<TileDataSO> result = new List<TileDataSO>();
        foreach (var item in currentShopItems)
            if (item is TileDataSO t) result.Add(t);
        return result;
    }

    public bool IsOutOfStock(ITradable item)
    {
        return stockRemaining.TryGetValue(item, out int s) && s == 0;
    }

    // ─── 구매 ─────────────────────────────────────────────────────────

    public void TryBuyItem(ITradable item)
    {
        if (item == null) return;

        if (IsOutOfStock(item))
        {
            Debug.LogWarning($"[Shop] {item.Name} 품절!");
            OnPurchaseFailed?.Invoke();
            return;
        }

        List<ResourceCost> costs = item.GetCosts();

        if (EconomyManager.Instance.TrySpend(costs))
        {
            InventoryManager.Instance.AddItem(item, 1);

            // 재고 차감
            if (stockRemaining.TryGetValue(item, out int stock) && stock > 0)
                stockRemaining[item] = stock - 1;

            OnPurchaseSuccess?.Invoke(item.Name);

            // 품절 시 상점 슬롯에서 제거
            if (IsOutOfStock(item))
                currentShopItems.Remove(item);
        }
        else
        {
            OnPurchaseFailed?.Invoke();
        }
    }

    // ─── 슬롯 수 조정 ─────────────────────────────────────────────────

    public void AddShopSlot(int amount = 1) => activeSlotCount = Mathf.Max(1, activeSlotCount + amount);
    public int ActiveSlotCount => activeSlotCount;
}