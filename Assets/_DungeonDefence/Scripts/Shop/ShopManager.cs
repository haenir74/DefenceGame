using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Panex.Inventory;

public class ShopManager : Singleton<ShopManager>
{
    [SerializeField] private List<UnitDataSO> unitCatalog;
    [SerializeField] private List<TileDataSO> tileCatalog;
    [SerializeField] private int rerollCost = 50;

    public List<UnitDataSO> GetUnitCatalog() => unitCatalog;
    public List<TileDataSO> GetTileCatalog() => tileCatalog;

    public event Action<string> OnPurchaseSuccess;
    public event Action OnPurchaseFailed;
    public event Action OnShopRefreshed;
    
    public void RerollShop()
    {
        if (EconomyManager.Instance.CanAfford(new List<ResourceCost>{ new ResourceCost{ type = CurrencyType.Gold, amount = rerollCost } }))
        {
            EconomyManager.Instance.AddCurrency(CurrencyType.Gold, -rerollCost);
            OnShopRefreshed?.Invoke();
        }
    }

    public void TryBuyItem(ITradable item)
    {
        if (item == null) return;

        List<ResourceCost> costs = item.GetCosts();

        if (EconomyManager.Instance.TrySpend(costs))
        {
            InventoryManager.Instance.AddItem(item, 1);
            OnPurchaseSuccess?.Invoke(item.Name);
        }
        else
        {
            OnPurchaseFailed?.Invoke();
        }
    }
}