using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;

public class ShopManager : Singleton<ShopManager>
{
    public event System.Action<string> OnPurchaseSuccess;
    public event System.Action OnPurchaseFailed;

    public void BuyUnit(UnitDataSO unitData)
    {
        TryBuyItem(unitData, unitData.cost);
    }

    public void BuyTile(TileDataSO tileData)
    {
        TryBuyItem(tileData, tileData.cost);
    }

    private void TryBuyItem(IStorable item, int cost)
    {
        if (item == null) return;
        if (EconomyManager.Instance.CanAfford(cost))
        {
            if (EconomyManager.Instance.TrySpendGold(cost))
            {
                InventoryManager.Instance.AddItem(item, 1);
                
                Debug.Log($"[Shop] 구매 성공: {item.Name} (-{cost} G)");
                OnPurchaseSuccess?.Invoke(item.Name);
            }
        }
        else
        {
            Debug.LogWarning("[Shop] 골드가 부족합니다.");
            OnPurchaseFailed?.Invoke();
        }
    }
}