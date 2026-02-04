using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameState : BaseState<GameController>
{
    public virtual void OnClickNode(GridNode node) { }
    public virtual void OnRightClickNode(GridNode node) { }
    public virtual void OnCancel() { }
}

public class NormalState : GameState
{
    public override void OnEnter(GameController controller) 
    { 
        // 상태 진입 로직
    }

    public override void OnClickNode(GridNode node) 
    {
        Debug.Log($"[Normal] 선택된 노드: {node.Coordinate}");
        var unit = UnitManager.Instance.SpawnUnit(null, node); // 예시 코드
    }
}

public class PlacementState : GameState
{
    private int itemId; 

    public PlacementState(int itemId)
    {
        this.itemId = itemId;
    }

    public override void OnClickNode(GridNode node)
    {
        if (GridManager.Instance.IsValidNode(node.X, node.Y))
        {
            Debug.Log($"[Placement] 아이템 {this.itemId} 설치 시도");
            Controller.ChangeState(new NormalState());
        }
    }

    public override void OnCancel()
    {
        Controller.ChangeState(new NormalState());
    }
}