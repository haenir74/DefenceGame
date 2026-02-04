using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameState : BaseState<GameManager>
{
    public virtual void OnClickNode(GridNode node) { }
    public virtual void OnCancel() { }
}

public class NormalState : GameState
{
    public override void Enter() 
    { 
        Debug.Log("Game Mode: Normal"); 
    }

    public override void OnClickNode(GridNode node) 
    {
        Debug.Log($"[Normal] Node Selected: {node.X}, {node.Y}");
    }
}

public class PlacementState : GameState
{
    private int _itemId; 

    public PlacementState(int itemId)
    {
        _itemId = itemId;
    }

    public override void Enter() 
    { 
        Debug.Log($"Game Mode: Placement (Item: {_itemId})"); 
    }

    public override void Exit()
    {
        
    }

    public override void OnClickNode(GridNode node)
    {
        Debug.Log($"[Placement] Trying to build item {_itemId} at {node.X},{node.Y}");
        Controller.ChangeState(new NormalState());
    }

    public override void OnCancel()
    {
        Controller.ChangeState(new NormalState());
    }
}