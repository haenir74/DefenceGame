using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;
using Panex.Inventory.Controller;

public class InventoryManager : Singleton<InventoryManager>
{
    [Header("Settings")]
    [SerializeField] private InventoryController unitInventoryController;
    [SerializeField] private InventoryController tileInventoryController;
    [SerializeField] private Settings unitSettings; 
    [SerializeField] private Settings tileSettings;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        if (unitInventoryController != null && unitSettings != null)
        {
            unitInventoryController.Configure(unitSettings);
        }
        if (tileInventoryController != null && tileSettings != null)
        {
            tileInventoryController.Configure(tileSettings);
        }
    }

    private InventoryController GetControllerFor(IStorable item)
    {
        if (item == null) return null;

        if (item is UnitDataSO)
        {
            return unitInventoryController;
        }
        else if (item is TileDataSO)
        {
            return tileInventoryController;
        }
        return null;
    }

    public void AddItem(IStorable item, int amount = 1)
    {
        var controller = GetControllerFor(item);
        
        if (controller != null)
        {
            int remaining = controller.AddItem(item, amount);
        }
    }

    public bool TryConsumeItem(IStorable item, int amount = 1)
    {
        var controller = GetControllerFor(item);
        if (controller != null)
        {
            return controller.RemoveItem(item, amount);
        }
        return false;
    }

    public int GetItemAmount(IStorable item)
    {
        var controller = GetControllerFor(item);
        if (controller != null)
        {
            return controller.GetItemAmount(item.ID);
        }
        return 0;
    }
}