using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{

    [SerializeField] private GridDataSO gridData;
    [SerializeField] private GridView gridView;
    [SerializeField] private Transform tileContainer;

    private GridMap map;
    private GridSystem system;

    public event System.Action<int> OnAttractivenessChanged;

    public GridDataSO Data => gridData;
    public GridMap Map => map;

    public void Initialize()
    {
        this.system = new GridSystem();
        this.map = new GridMap();

        if (this.gridData != null)
        {
            this.system.Generate(this.map, this.gridData);
            BuildView();
            this.system.CalculateFlowField(this.map, this.map.CoreNode);
            this.system.CalculateAttractivenessInfluence(this.map);
            OnAttractivenessChanged?.Invoke(CalculateTotalAttractiveness());
        }
    }

    private void BuildView()
    {
        if (this.map == null || this.gridData == null) return;
        CalculateCellSize();
        PlaceInitialTiles();

        if (this.gridView != null)
            this.gridView.Setup(this.map, this.gridData);
    }

    private void CalculateCellSize()
    {
        if (this.gridData.defaultTilePrefab == null) return;
        Renderer rend = this.gridData.defaultTilePrefab.GetComponentInChildren<Renderer>();
        this.gridData.cellSize = rend != null ? rend.bounds.size.x : this.gridData.defaultTilePrefab.transform.localScale.x;
    }

    private void PlaceInitialTiles()
    {
        if (this.map == null || this.tileContainer == null) return;
        int layerIndex = 0;
        int mask = gridData.groundLayer.value;
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                layerIndex = i;
                break;
            }
        }

        foreach (var node in this.map.Nodes)
        {
            GameObject cellObj = new GameObject($"Cell_{node.X}_{node.Y}");
            cellObj.transform.SetParent(this.tileContainer);
            cellObj.transform.position = node.WorldPosition;
            cellObj.layer = layerIndex;

            BoxCollider col = cellObj.AddComponent<BoxCollider>();
            col.size = new Vector3(this.gridData.cellSize, 0.1f, this.gridData.cellSize);
            col.center = Vector3.zero;

            GridTile tileComp = cellObj.AddComponent<GridTile>();
            TileView tileView = cellObj.AddComponent<TileView>();
            GridDropHandler dropHandler = cellObj.AddComponent<GridDropHandler>();
            dropHandler.TargetNode = node;

            node.Tile = tileComp;

            TileDataSO initialTileData = this.gridData.defaultTileData;
            if (node == map.CoreNode)
            {
                TileDataSO manaWell = Resources.Load<TileDataSO>("Data/Tiles/Tile_ManaWell");
                if (manaWell != null) initialTileData = manaWell;
            }

            tileComp.Setup(initialTileData);

            tileView.SetDefaultPrefab(this.gridData.defaultTilePrefab);
            tileView.Setup(node, initialTileData);

            node.CurrentTile = tileView;
            node.SetTileData(initialTileData);
        }
    }

    public void ChangeTile(GridNode node, TileDataSO newData)
    {
        if (node == null || newData == null) return;
        if (node.Tile != null)
        {
            node.Tile.Setup(newData);
        }
        if (node.CurrentTile != null)
        {
            node.CurrentTile.UpdateVisual(newData);
        }
        node.SetTileData(newData);

        if (this.system != null)
        {
            this.system.CalculateFlowField(this.map, this.map.CoreNode);
            this.system.CalculateAttractivenessInfluence(this.map);
            OnAttractivenessChanged?.Invoke(CalculateTotalAttractiveness());
        }
    }

    public void UpdateHoverView(GridNode prevNode, GridNode currNode)
    {
        if (prevNode != null && prevNode.CurrentTile != null)
            prevNode.CurrentTile.SetHighlight(false);

        if (currNode != null && currNode.CurrentTile != null)
            currNode.CurrentTile.SetHighlight(true);
    }

    public GridNode GetNode(Vector3 worldPos)
    {
        return this.system?.GetNode(this.map, this.gridData, worldPos);
    }

    public GridNode GetNode(int x, int y)
    {
        return this.map?.GetNode(x, y);
    }

    public GridNode GetCoreNode() => this.map?.CoreNode;
    public GridNode GetSpawnNode() => this.map?.SpawnNode;

    public bool IsValidNode(int x, int y) => this.map != null && this.map.IsValid(x, y);

    public TileDataSO GetTileData(Vector2Int coordinate)
    {
        GridNode node = GetNode(coordinate.x, coordinate.y);
        return node?.CurrentTileData;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        if (this.system == null || this.gridData == null) return Vector3.zero;
        return this.system.GetWorldPosition(x, y, this.gridData.cellSize);
    }

    public void OnHoverChanged(GridNode prevNode, GridNode currNode)
    {
        UpdateHoverView(prevNode, currNode);
    }

    public Vector2Int GetNextPosition(Vector2Int currentPos)
    {
        return currentPos;
    }

    public List<GridNode> GetNeighbors(GridNode node)
    {
        List<GridNode> neighbors = new List<GridNode>();
        if (map == null || node == null) return neighbors;

        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = node.X + dx[i];
            int ny = node.Y + dy[i];

            if (IsValidNode(nx, ny))
            {
                neighbors.Add(GetNode(nx, ny));
            }
        }
        return neighbors;
    }
    public GameObject GetTileGameObject(Vector2Int coord)
    {
        if (tileContainer == null) return null;
        Transform tile = tileContainer.Find($"Cell_{coord.x}_{coord.y}");
        return tile != null ? tile.gameObject : null;
    }
    public int CalculateTotalAttractiveness()
    {
        if (this.map == null) return 0;
        int total = 0;
        foreach (var node in this.map.Nodes)
        {
            total += node.GetTileBonus();
        }
        return total;
    }
}



