using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EconomyManager : Singleton<EconomyManager>
{
    [Header("Settings")]
    [SerializeField] private int startingGold = 100;

    public int CurrentGold { get; private set; }

    public event Action<int> OnGoldChanged;

    private void Start()
    {
        CurrentGold = startingGold;
        NotifyGoldChanged();
    }

    public void AddGold(int amount)
    {
        CurrentGold += amount;
        NotifyGoldChanged();
    }

    public bool CanAfford(int amount)
    {
        return CurrentGold >= amount;
    }

    public bool TrySpendGold(int amount)
    {
        if (CanAfford(amount))
        {
            CurrentGold -= amount;
            NotifyGoldChanged();
            return true;
        }
        return false;
    }

    private void NotifyGoldChanged()
    {
        OnGoldChanged?.Invoke(CurrentGold);
    }
}