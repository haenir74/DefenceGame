using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;

public enum CurrencyType
{
    Gold,
}

[System.Serializable]
public struct ResourceCost
{
    public CurrencyType type;
    public int amount;
}

public interface ITradable : IStorable
{
    List<ResourceCost> GetCosts();
}