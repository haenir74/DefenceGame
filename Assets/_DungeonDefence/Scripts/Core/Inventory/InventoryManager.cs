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

    public IStorable CurrentSelectedItem { get; private set; }

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
        if (item is UnitDataSO unitData)
        {
            GameManager.Instance.SelectUnitToPlace(unitData);
        }
        if (item is TileDataSO tileData)
        {
            GameManager.Instance.SelectTileToPlace(tileData);
        }
    }

    private void SetPlacementMode(IStorable item)
    {
        CurrentSelectedItem = item;

        // 예시: InputManager나 GameManager에 '배치 모드' 시작을 알림
        // 만약 InputManager에 SetPlacementItem 같은 함수가 있다면 호출해야 합니다.
        // InputManager.Instance.BeginPlacement(item); 
        
        Debug.Log($"배치 모드 활성화: {item.Name} 선택됨");
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