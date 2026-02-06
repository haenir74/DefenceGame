using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;
using Panex.Inventory.Controller;

public class InventoryManager : Singleton<InventoryManager>
{
    [Header("Settings")]
    [SerializeField] private Settings unitInventorySettings; 
    [SerializeField] private Settings tileInventorySettings;
    [SerializeField] private InventoryController unitInventoryPrefab;
    [SerializeField] private InventoryController tileInventoryPrefab;

    [Header("UI")]
    [SerializeField] private Transform unitListParent;
    [SerializeField] private Transform tileListParent;

    private Inventory unitInventory;
    private Inventory tileInventory;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        unitInventory = CreateListInventory("UnitInventory", unitInventorySettings, unitInventoryPrefab, unitListParent);
        tileInventory = CreateListInventory("TileInventory", tileInventorySettings, tileInventoryPrefab, tileListParent);
    }

    private Inventory CreateListInventory(string id, Settings settings, InventoryController prefab, Transform parent)
    {
        var inv = new Inventory(id, settings, prefab, parent);

        var helper = parent.GetComponent<InventoryListHelper>();
        if (helper == null)
        {
            helper = parent.gameObject.AddComponent<InventoryListHelper>();
        }

        inv.OnSlotClicked += HandleSlotClicked;
        inv.OnInventoryChanged += () => helper.RefreshSlots();

        inv.Open();
        helper.RefreshSlots();

        return inv;
    }

    public void AddItem(IStorable item, int amount = 1)
    {
        if (item is UnitDataSO)
        {
            unitInventory?.AddItem(item, amount);
            Debug.Log($"[Unit List] {item.Name} 획득");
        }
        else if (item is TileDataSO)
        {
            tileInventory?.AddItem(item, amount);
            Debug.Log($"[Tile List] {item.Name} 획득");
        }
    }

    public void RemoveItem(IStorable item, int amount = 1)
    {
        if (item is UnitDataSO)
            unitInventory?.RemoveItem(item, amount);
        else if (item is TileDataSO)
            tileInventory?.RemoveItem(item, amount);
    }

    public void ToggleAllInventories()
    {
        unitInventory?.Toggle();
        tileInventory?.Toggle();
    }

    private void HandleSlotClicked(IStorable item, int amount)
    {
        if (item is UnitDataSO unitData)
        {
            Debug.Log($"[Unit] 유닛 배치: {unitData.Name}");
            // GameManager.Instance.StartUnitPlacement(unitData);
        }
        else if (item is TileDataSO tileData)
        {
            Debug.Log($"[Tile] 타일 배치: {tileData.Name}");
            // GameManager.Instance.StartTilePlacement(tileData);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (unitInventory != null) unitInventory.Destroy();
        if (tileInventory != null) tileInventory.Destroy();
    }
}