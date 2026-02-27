using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EconomyManager : Singleton<EconomyManager>
{
    private Dictionary<CurrencyType, int> currencies = new Dictionary<CurrencyType, int>();
    public event Action<CurrencyType, int, int> OnCurrencyChanged;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Start()
    {

    }

    private void Initialize()
    {
        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            currencies[type] = 0;
        }
    }

    public void Reset()
    {
        Initialize();
    }

    public int GetCurrencyAmount(CurrencyType type)
    {
        if (currencies.ContainsKey(type))
            return currencies[type];
        return 0;
    }

    public void AddCurrency(CurrencyType type, int amount)
    {
        if (amount < 0) return;

        if (currencies.ContainsKey(type))
        {
            currencies[type] += amount;
        }
        else
        {
            currencies.Add(type, amount);
        }

        OnCurrencyChanged?.Invoke(type, currencies[type], amount);
    }

    public bool CanAfford(List<ResourceCost> costs)
    {
        if (costs == null || costs.Count == 0) return true;

        foreach (var cost in costs)
        {
            int currentAmount = GetCurrencyAmount(cost.type);
            if (currentAmount < cost.amount)
            {
                return false;
            }
        }
        return true;
    }

    public bool TrySpend(List<ResourceCost> costs)
    {
        if (!CanAfford(costs)) return false;
        foreach (var cost in costs)
        {
            currencies[cost.type] -= cost.amount;
            OnCurrencyChanged?.Invoke(cost.type, currencies[cost.type], -cost.amount);
        }

        return true;
    }

}


