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
        this.CurrentGold = this.startingGold;
        NotifyGoldChanged();
    }

    public void AddGold(int amount)
    {
        this.CurrentGold += amount;
        NotifyGoldChanged();
    }

    public bool CanAfford(int amount)
    {
        return this.CurrentGold >= amount;
    }

    public bool TrySpendGold(int amount)
    {
        if (CanAfford(amount))
        {
            this.CurrentGold -= amount;
            NotifyGoldChanged();
            return true;
        }
        return false;
    }

    private void NotifyGoldChanged()
    {
        this.OnGoldChanged?.Invoke(this.CurrentGold);
    }
}