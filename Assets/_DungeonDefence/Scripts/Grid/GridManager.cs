using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    private GridController controller;

    public GridDataSO Data => controller != null ? controller.Data : null;

    public void Initialize(GridController controller)
    {
        this.controller = controller;
        if (this.controller != null)
        {
            this.controller.Initialize();
        }
    }

    public GridNode GetNode(Vector3 worldPos)
    {
        return this.controller?.GetNode(worldPos);
    }

    public GridNode GetNode(int x, int y)
    {
        return this.controller?.GetNode(x, y);
    }

    public GridNode GetCoreNode() => this.controller?.Map?.CoreNode;
    public GridNode GetSpawnNode() => this.controller?.Map?.SpawnNode;

    public bool IsValidNode(int x, int y) 
    {
        return this.controller != null && this.controller.IsValidNode(x, y);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return this.controller != null ? this.controller.GetWorldPosition(x, y) : Vector3.zero;
    }

    public void OnHoverChanged(GridNode prevNode, GridNode currNode)
    {
        this.controller?.UpdateHoverView(prevNode, currNode);
    }

    public Vector2Int GetNextPosition(Vector2Int currentPos)
    {
        return this.controller != null ? this.controller.GetNextPosition(currentPos) : currentPos;
    }
}
