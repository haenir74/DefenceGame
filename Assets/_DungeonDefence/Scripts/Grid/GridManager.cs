using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    private GridController _controller;
    private GridSystem _system;
    private GridMap _map;

    public GridDataSO Data => _controller != null ? _controller.Data : null;

    protected override void Awake()
    {
        base.Awake();
        _system = new GridSystem();
        _map = new GridMap();
    }

    public void Initialize(GridController controller)
    {
        this._controller = controller;
        if (_controller == null || _controller.Data == null) return;

        _system.Generate(_map, _controller.Data);
        _controller.BuildView(_map);
    }

    public GridNode GetNode(Vector3 worldPos)
    {
        if (_controller == null || _system == null) return null;
        return _system.GetNode(_map, _controller.Data, worldPos);
    }

    public GridNode GetNode(int x, int y)
    {
        if (_map == null) return null;
        return _map.GetNode(x, y);
    }

    public GridNode GetCoreNode() => _map?.CoreNode;
    public GridNode GetSpawnNode() => _map?.SpawnNode;

    public bool IsValidNode(int x, int y) => _map.IsValid(x, y);

    public Vector3 GetWorldPosition(int x, int y)
    {
        if (_controller == null || _system == null) return Vector3.zero;
        return _system.GetWorldPosition(x, y, _controller.Data.cellSize);
    }

    public List<GridNode> GetNeighbors(GridNode node)
    {
        return _system.GetNeighbors(_map, node);
    }

    public void ChangeTile(GridNode node, TileDataSO newData)
    {
        if (node == null || newData == null) return;

        if (node.Tile != null)
        {
            node.Tile.Setup(newData);
        }
    }

    public void OnHoverChanged(GridNode prevNode, GridNode currNode)
    {
        _controller?.UpdateHoverView(prevNode, currNode);
    }
}
