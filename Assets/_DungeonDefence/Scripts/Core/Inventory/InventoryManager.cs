using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;
using Panex.Inventory.Controller;

public class InventoryManager : Singleton<InventoryManager>
{

    [SerializeField] private InventoryController unitInventoryController;
    [SerializeField] private InventoryController tileInventoryController;
    [SerializeField] private Settings unitSettings;
    [SerializeField] private Settings tileSettings;

    public IStorable CurrentSelectedItem { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        if (unitInventoryController != null && unitSettings != null)
        {
            unitInventoryController.Configure(unitSettings);
            unitInventoryController.OnSlotClicked += OnSlotClicked;
        }
        if (tileInventoryController != null && tileSettings != null)
        {
            tileInventoryController.Configure(tileSettings);
            tileInventoryController.OnSlotClicked += OnSlotClicked;
        }
    }

    private void OnSlotClicked(IStorable item, int index)
    {


    }

    public void Reset()
    {
        if (unitInventoryController != null) unitInventoryController.Clear();
        if (tileInventoryController != null) tileInventoryController.Clear();
    }

    private void SetPlacementMode(IStorable item)
    {
        CurrentSelectedItem = item;






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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (unitInventoryController != null)
            unitInventoryController.OnSlotClicked -= OnSlotClicked;

        if (tileInventoryController != null)
            tileInventoryController.OnSlotClicked -= OnSlotClicked;
    }
}


