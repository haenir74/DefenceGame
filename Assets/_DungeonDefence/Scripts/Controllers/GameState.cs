using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory.Controller;

public interface IGameState : IState
{
    void OnClickNode(Node node);
    void OnCancel();
}

public class NormalState : IGameState
{
    public void Enter() { Debug.Log("Normal Mode"); }
    public void Execute() { }
    public void Exit() { }
    
    public void OnClickNode(Node node) 
    {
        Debug.Log($"Node Clicked: {node.X}, {node.Y}");
    }
    public void OnCancel() { }
}

public class PlacementState : IGameState
{
    private BaseItemSO itemToPlace;
    private InventoryController sourceInventory;
    private int sourceSlotIndex;

    public PlacementState(BaseItemSO item, InventoryController inventory, int slotIndex)
    {
        this.itemToPlace = item;
        this.sourceInventory = inventory;
        this.sourceSlotIndex = slotIndex;
    }

    public void Enter() 
    { 
        Debug.Log($"[Placement Mode] Item: {itemToPlace.Name}, Slot: {sourceSlotIndex}");
    }

    public void Execute() 
    {
    
    }

    public void Exit()
    {
        
    }

    public void OnClickNode(Node node)
    {
        bool success = BuildManager.Instance.TryPlaceItem(node, itemToPlace);
        
        if (success)
        {
            ConsumeItemByIndex();
            GameManager.Instance.ChangeState(new NormalState());
        }
    }

    public void OnCancel()
    {
        GameManager.Instance.ChangeState(new NormalState());
    }

    private void ConsumeItemByIndex()
    {
        if (sourceInventory == null) return;

        var slot = sourceInventory.GetSlot(sourceSlotIndex);

        if (slot != null && !slot.IsEmpty)
        {
            if (slot.Amount > 1)
            {
                sourceInventory.SetItem(sourceSlotIndex, slot.ItemData, slot.Amount - 1);
            }
            else
            {
                sourceInventory.RemoveItem(sourceSlotIndex);
            }
        }
    }
}