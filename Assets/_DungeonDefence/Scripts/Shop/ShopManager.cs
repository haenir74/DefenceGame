using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Panex.Inventory;

public class ShopManager : Singleton<ShopManager>
{
    
    
    [SerializeField] private int activeSlotCount = 4;
    [SerializeField] private int rerollCost = 50;

    
    
    [SerializeField] private ShopUnlockPoolSO unlockPool;
    
    [SerializeField] private List<UnitDataSO> defaultUnlockedUnits = new List<UnitDataSO>();
    [SerializeField] private List<TileDataSO> defaultUnlockedTiles = new List<TileDataSO>();

    private List<ITradable> unlockedItems = new List<ITradable>();

    private Dictionary<ITradable, int> stockRemaining = new Dictionary<ITradable, int>();

    private List<ITradable> currentShopItems = new List<ITradable>();

    public event Action<string> OnPurchaseSuccess;
    public event Action OnPurchaseFailed;
    public event Action OnShopRefreshed;

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

    public void UnlockItem(ITradable item)
    {
        if (item == null || unlockedItems.Contains(item)) return;
        unlockedItems.Add(item);

        int stock = GetInitialStock(item);
        stockRemaining[item] = stock <= 0 ? -1 : stock;
    }

    private int GetInitialStock(ITradable item)
    {
        if (item is UnitDataSO unit) return unit.shopStock;
        if (item is TileDataSO tile) return tile.shopStock;
        return -1;
    }

    public List<ITradable> GetUnlockCandidates(int count = 3)
    {
        if (unlockPool == null) return new List<ITradable>();
        return unlockPool.GetRandomCandidates(count, unlockedItems);
    }

    public void RollShopItems(TierProbabilities tierProbs = null)
    {
        currentShopItems.Clear();

        
        int perkBonus = (MetaManager.Instance != null) ? MetaManager.Instance.GetPerkLevel("ShopSlot") : 0;
        int totalSlots = activeSlotCount + perkBonus;

        List<ITradable> available = new List<ITradable>();
        foreach (var item in unlockedItems)
        {
            if (!stockRemaining.TryGetValue(item, out int stock)) continue;
            if (stock == -1 || stock > 0) available.Add(item);
        }

        List<ITradable> picked;
        if (tierProbs != null)
        {
            picked = RollWithTierWeights(available, totalSlots, tierProbs);
        }
        else
        {
            for (int i = available.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (available[i], available[j]) = (available[j], available[i]);
            }
            int take = Mathf.Min(totalSlots, available.Count);
            picked = available.GetRange(0, take);
        }

        currentShopItems.AddRange(picked);
        OnShopRefreshed?.Invoke();
    }

    private List<ITradable> RollWithTierWeights(List<ITradable> pool, int count, TierProbabilities probs)
    {

        var byTier = new System.Collections.Generic.Dictionary<UnitTier, List<ITradable>>
        {
            { UnitTier.Basic,        new List<ITradable>() },
            { UnitTier.Intermediate, new List<ITradable>() },
            { UnitTier.Advanced,     new List<ITradable>() },
            { UnitTier.Supreme,      new List<ITradable>() },
        };

        foreach (var item in pool)
        {

            UnitTier tier = item is UnitDataSO u ? u.tier : UnitTier.Basic;
            byTier[tier].Add(item);
        }

        int totalWeight = probs.basicWeight + probs.intermediateWeight + probs.advancedWeight + probs.supremeWeight;
        if (totalWeight <= 0) totalWeight = 1;

        var result = new List<ITradable>();
        var used = new HashSet<ITradable>();

        for (int i = 0; i < count; i++)
        {
            int roll = UnityEngine.Random.Range(0, totalWeight);
            UnitTier selected;
            if (roll < probs.basicWeight)
                selected = UnitTier.Basic;
            else if (roll < probs.basicWeight + probs.intermediateWeight)
                selected = UnitTier.Intermediate;
            else if (roll < probs.basicWeight + probs.intermediateWeight + probs.advancedWeight)
                selected = UnitTier.Advanced;
            else
                selected = UnitTier.Supreme;

            ITradable candidate = PickRandom(byTier[selected], used);

            if (candidate == null)
            {
                foreach (var kv in byTier)
                {
                    candidate = PickRandom(kv.Value, used);
                    if (candidate != null) break;
                }
            }

            if (candidate != null)
            {
                result.Add(candidate);
                used.Add(candidate);
            }
        }
        return result;
    }

    private ITradable PickRandom(List<ITradable> pool, HashSet<ITradable> exclude)
    {
        var valid = pool.FindAll(x => !exclude.Contains(x));
        if (valid.Count == 0) return null;
        return valid[UnityEngine.Random.Range(0, valid.Count)];
    }

    public void ResetStocksForNewWave()
    {
        var keys = new List<ITradable>(stockRemaining.Keys);
        foreach (var item in keys)
        {
            int stock = GetInitialStock(item);
            stockRemaining[item] = stock <= 0 ? -1 : stock;
        }
        
    }

    public void RerollShop()
    {
        var cost = new List<ResourceCost> { new ResourceCost { type = CurrencyType.Gold, amount = rerollCost } };
        if (EconomyManager.Instance.TrySpend(cost))
        {
            RollShopItems();
        }
        else
        {
            
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


