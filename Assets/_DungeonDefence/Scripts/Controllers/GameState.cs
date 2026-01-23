using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public PlacementState(BaseItemSO item)
    {
        this.itemToPlace = item;
    }

    public void Enter() 
    { 
        Debug.Log($"Placement Mode: {itemToPlace.ID}");
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
            GameManager.Instance.ConsumeInventoryItem(itemToPlace);
            GameManager.Instance.ChangeState(new NormalState());
        }
    }

    public void OnCancel()
    {
        GameManager.Instance.ChangeState(new NormalState());
    }
}