using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispatchManager : Singleton<DispatchManager>
{


    public int baseDispatchReward = 100;



    public float playerDispatchMultiplier = 1.0f;

    public event System.Action OnDispatchStateChanged;

    public class DispatchEntry
    {
        public Unit Unit;
        public UnitDataSO Data;
        public bool IsEmpty => Unit == null && Data == null;
    }

    private List<DispatchEntry> dispatchSlots = new List<DispatchEntry>();
    public IReadOnlyList<DispatchEntry> DispatchSlots => dispatchSlots;

    public bool RequestAssignUnit(int slotIndex, Unit unit)
    {
        if (unit == null || !unit.IsPlayerTeam || unit.Data.category == UnitCategory.Core) return false;

        unit.SetDispatchMode(true);
        SetEntry(slotIndex, new DispatchEntry { Unit = unit });
        return true;
    }

    public bool RequestAssignData(int slotIndex, UnitDataSO data)
    {
        if (data == null) return false;
        SetEntry(slotIndex, new DispatchEntry { Data = data });
        return true;
    }

    private void SetEntry(int slotIndex, DispatchEntry entry)
    {
        if (slotIndex < 0 || slotIndex >= dispatchSlots.Count)
            dispatchSlots.Add(entry);
        else
            dispatchSlots[slotIndex] = entry;
        OnDispatchStateChanged?.Invoke();
    }

    public void RequestRecall(int slotIndex, bool returnToInventory = true)
    {
        if (slotIndex >= 0 && slotIndex < dispatchSlots.Count)
        {
            var entry = dispatchSlots[slotIndex];
            if (entry.Unit != null)
                entry.Unit.SetDispatchMode(false);
            else if (entry.Data != null && returnToInventory)
                InventoryManager.Instance?.AddItem(entry.Data, 1);

            dispatchSlots.RemoveAt(slotIndex);
            OnDispatchStateChanged?.Invoke();
        }
    }

    public void Initialize()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveCompleted += HandleWaveCompleted;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveCompleted -= HandleWaveCompleted;
        }
    }

    private void HandleWaveCompleted()
    {
        int bonus = CalculateTotalBonus();
        if (bonus > 0 && EconomyManager.InstanceExists)
        {
            EconomyManager.Instance.AddCurrency(CurrencyType.Gold, bonus);
        }
    }

    public int CalculateUnitBonus(Unit unit = null, UnitDataSO data = null)
    {
        UnitDataSO sourceData = unit != null ? unit.Data : data;
        if (sourceData == null) return 0;

        float efficiency = sourceData.dispatchEfficiency;
        float reward = baseDispatchReward * efficiency * playerDispatchMultiplier;

        return Mathf.RoundToInt(reward);
    }

    public int CalculateTotalBonus()
    {
        int total = 0;
        foreach (var entry in dispatchSlots)
        {
            if (!entry.IsEmpty) total += CalculateUnitBonus(entry.Unit, entry.Data);
        }
        return total;
    }

    public void AddPlayerEfficiency(float amount)
    {
        playerDispatchMultiplier += amount;
    }
}



