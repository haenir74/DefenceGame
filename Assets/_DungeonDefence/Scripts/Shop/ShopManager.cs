using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Panex.Inventory;

public class ShopManager : Singleton<ShopManager>
{
    [SerializeField] private int activeSlotCount = 4;
    [SerializeField] private int rerollCost = 50;

    [SerializeField] private ShopUnlockPoolSO unlockPool;

    private Dictionary<ITradable, int> stockRemaining = new Dictionary<ITradable, int>();
    private List<ITradable> currentShopItems = new List<ITradable>();

    public event Action<string> OnPurchaseSuccess;
    public event Action OnPurchaseFailed;
    public event Action OnShopRefreshed;

    private Dictionary<UnitTier, List<ITradable>> categoricalPool = new Dictionary<UnitTier, List<ITradable>>();

    private void Start()
    {
        InitializeCategoricalPool();
        InitializeStocks();
        RollShopItems();
    }

    private void InitializeCategoricalPool()
    {
        categoricalPool.Clear();
        categoricalPool[UnitTier.Basic] = new List<ITradable>();
        categoricalPool[UnitTier.Intermediate] = new List<ITradable>();
        categoricalPool[UnitTier.Advanced] = new List<ITradable>();
        categoricalPool[UnitTier.Supreme] = new List<ITradable>();

        if (unlockPool == null) return;

        foreach (var unit in unlockPool.unitCandidates)
        {
            if (unit != null) categoricalPool[unit.tier].Add(unit);
        }
        foreach (var tile in unlockPool.tileCandidates)
        {
            if (tile != null) categoricalPool[tile.tier].Add(tile);
        }
    }

    private void InitializeStocks()
    {
        stockRemaining.Clear();
        if (unlockPool == null) return;

        foreach (var unit in unlockPool.unitCandidates)
            if (unit != null) SetInitialStock(unit);
        foreach (var tile in unlockPool.tileCandidates)
            if (tile != null) SetInitialStock(tile);
    }

    private void SetInitialStock(ITradable item)
    {
        int stock = GetInitialStock(item);
        stockRemaining[item] = stock <= 0 ? -1 : stock;
    }

    public void Reset()
    {
        stockRemaining.Clear();
        currentShopItems.Clear();
        InitializeCategoricalPool();
        InitializeStocks();
        RollShopItems();
    }

    public void ResetWithProbabilities(TierProbabilities probs)
    {
        stockRemaining.Clear();
        currentShopItems.Clear();
        InitializeCategoricalPool();
        InitializeStocks();
        RollShopItems(probs);
    }

    private int GetInitialStock(ITradable item)
    {
        if (item is UnitDataSO unit) return unit.shopStock;
        if (item is TileDataSO tile) return tile.shopStock;
        return -1;
    }

    public void RollShopItems(TierProbabilities tierProbs = null)
    {
        currentShopItems.Clear();

        if (tierProbs == null && WaveManager.Instance != null)
        {
            tierProbs = WaveManager.Instance.GetNextWaveTierProbs();
        }

        if (tierProbs == null)
        {
            tierProbs = new TierProbabilities();
        }

        int perkBonus = (MetaManager.Instance != null) ? MetaManager.Instance.GetPerkLevel("ShopSlot") : 0;
        int totalSlots = activeSlotCount + perkBonus;

        int totalWeight = tierProbs.basicWeight + tierProbs.intermediateWeight + tierProbs.advancedWeight + tierProbs.supremeWeight;
        if (totalWeight <= 0)
        {
            Debug.LogWarning("ShopManager: Total weight is 0. Check TierProbabilities or WaveDataSO configuration.");
            totalWeight = 1;
        }

        if (categoricalPool.Count == 0 || categoricalPool.Values.All(list => list.Count == 0))
        {
            Debug.LogError("ShopManager: Categorical pool is empty! Shop will be empty. Check ShopUnlockPoolSO.");
        }

        HashSet<ITradable> used = new HashSet<ITradable>();

        for (int i = 0; i < totalSlots; i++)
        {
            int roll = UnityEngine.Random.Range(0, totalWeight);
            UnitTier selected;
            if (roll < tierProbs.basicWeight)
                selected = UnitTier.Basic;
            else if (roll < tierProbs.basicWeight + tierProbs.intermediateWeight)
                selected = UnitTier.Intermediate;
            else if (roll < tierProbs.basicWeight + tierProbs.intermediateWeight + tierProbs.advancedWeight)
                selected = UnitTier.Advanced;
            else
                selected = UnitTier.Supreme;

            ITradable candidate = PickRandomFromPool(selected, used);
            if (candidate == null)
            {

                foreach (UnitTier tier in (UnitTier[])Enum.GetValues(typeof(UnitTier)))
                {
                    candidate = PickRandomFromPool(tier, used);
                    if (candidate != null) break;
                }
            }

            if (candidate != null)
            {
                currentShopItems.Add(candidate);
                used.Add(candidate);
            }
        }

        OnShopRefreshed?.Invoke();
    }

    private ITradable PickRandomFromPool(UnitTier tier, HashSet<ITradable> exclude)
    {
        if (!categoricalPool.ContainsKey(tier)) return null;

        var valid = categoricalPool[tier].FindAll(x =>
            !exclude.Contains(x) &&
            (!stockRemaining.ContainsKey(x) || stockRemaining[x] == -1 || stockRemaining[x] > 0)
        );

        if (valid.Count == 0) return null;
        return valid[UnityEngine.Random.Range(0, valid.Count)];
    }

    public void ResetStocksForNewWave()
    {
        InitializeStocks();
    }

    public void RerollShop()
    {
        var cost = new List<ResourceCost> { new ResourceCost { type = CurrencyType.Gold, amount = rerollCost } };
        if (EconomyManager.Instance.TrySpend(cost))
        {
            RollShopItems();
        }
    }

    public List<ITradable> GetCurrentShopItems() => currentShopItems;

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

    public void TryBuyItem(ITradable item)
    {
        if (item == null) return;

        if (IsOutOfStock(item))
        {
            OnPurchaseFailed?.Invoke();
            return;
        }

        List<ResourceCost> costs = item.GetCosts();

        if (EconomyManager.Instance.TrySpend(costs))
        {
            InventoryManager.Instance.AddItem(item, 1);

            if (stockRemaining.TryGetValue(item, out int stock) && stock > 0)
                stockRemaining[item] = stock - 1;

            OnPurchaseSuccess?.Invoke(item.Name);

            if (IsOutOfStock(item))
                currentShopItems.Remove(item);
        }
        else
        {
            OnPurchaseFailed?.Invoke();
        }
    }

    public void AddShopSlot(int amount = 1) => activeSlotCount = Mathf.Max(1, activeSlotCount + amount);
    public int ActiveSlotCount => activeSlotCount;
}
