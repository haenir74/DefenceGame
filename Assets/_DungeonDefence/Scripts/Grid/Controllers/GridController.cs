using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private GridDataSO gridData;
    [SerializeField] private GridView gridView;
    [SerializeField] private Transform tileContainer;

    private GridMap map;
    private GridSystem system;
    private PathFinderSystem pathFinder;

    public GridDataSO Data => gridData;
    public GridMap Map => map;

    public void Initialize()
    {
        this.system = new GridSystem();
        this.map = new GridMap();
        this.pathFinder = new PathFinderSystem();

        if (this.gridData != null)
        {
            this.system.Generate(this.map, this.gridData);
            this.pathFinder.Initialize(this.map);
            BuildView();
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
        for(int i=0; i<32; i++) {
            if((mask & (1 << i)) != 0) {
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

            node.Tile = tileComp;
            tileComp.Setup(this.gridData.defaultTileData);
            
            tileView.SetDefaultPrefab(this.gridData.defaultTilePrefab);
            tileView.Setup(node, this.gridData.defaultTileData);
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

    public bool IsValidNode(int x, int y) => this.map != null && this.map.IsValid(x, y);

    public Vector3 GetWorldPosition(int x, int y)
    {
        if (this.system == null || this.gridData == null) return Vector3.zero;
        return this.system.GetWorldPosition(x, y, this.gridData.cellSize);
    }

    public Vector2Int GetNextPosition(Vector2Int currentPos)
    {
        return this.pathFinder != null ? this.pathFinder.GetNextStep(currentPos) : currentPos;
    }

    public List<GridNode> GetNeighbors(GridNode node)
    {
        return pathFinder != null ? pathFinder.GetNeighbors(node) : null;
    }
}