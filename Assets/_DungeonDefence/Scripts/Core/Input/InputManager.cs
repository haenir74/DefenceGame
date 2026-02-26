using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputManager : Singleton<InputManager>
{
    public event Action<GridNode> OnClickNode;
    public event Action<Unit> OnClickUnit;
    public event Action<GridNode> OnRightClickNode;
    public event Action<GridNode, GridNode> OnHoverNodeChanged;

    public event Action OnCancel;

    public void TriggerClick(GridNode node) => OnClickNode?.Invoke(node);
    public void TriggerUnitClick(Unit unit) => OnClickUnit?.Invoke(unit);
    public void TriggerRightClick(GridNode node) => OnRightClickNode?.Invoke(node);
    public void TriggerHover(GridNode prev, GridNode curr) => OnHoverNodeChanged?.Invoke(prev, curr);
    public void TriggerCancel() => OnCancel?.Invoke();
}



