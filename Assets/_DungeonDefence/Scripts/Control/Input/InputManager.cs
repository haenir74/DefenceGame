using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputManager : Singleton<InputManager>
{
    public event Action<GridNode> OnClickNode;
    public event Action<GridNode> OnRightClickNode;
    public event Action<GridNode, GridNode> OnHoverNodeChanged;

    public void TriggerClick(GridNode node) => OnClickNode?.Invoke(node);
    public void TriggerRightClick(GridNode node) => OnRightClickNode?.Invoke(node);
    public void TriggerHover(GridNode prev, GridNode curr) => OnHoverNodeChanged?.Invoke(prev, curr);
}
