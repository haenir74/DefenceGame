using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    [SerializeField] private List<ScriptableObject> itemsForSale; 

    public void TryBuy(ITradable item)
    {
        if (item == null) return;

        // if (EconomyManager.Instance.TrySpendGold(item.Cost))
        // {
        //     if (item is IStorable storableItem)
        //     {
        //         Debug.Log($"구매 성공: {item.Name}");
        //     }
        //     else
        //     {
        //         Debug.Log($"구매 성공(즉시 사용): {item.Name}");
        //     }
        // }
        // else
        // {
        //     Debug.Log("골드가 부족합니다.");
        // }
    }
}