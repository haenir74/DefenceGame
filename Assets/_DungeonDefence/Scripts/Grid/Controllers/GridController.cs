using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private GridDataSO gridData;
    [SerializeField] private GridView gridView;
    [SerializeField] private Transform tileContainer;

    public GridDataSO Data => gridData;

    public void BuildView(GridMap map)
    {
        CalculateCellSize();
        PlaceInitialTiles(map);
        if (gridView != null) 
            gridView.Setup(map, gridData);
    }

    private void CalculateCellSize()
    {
        if (gridData.defaultTilePrefab == null) return;
        
        Renderer rend = gridData.defaultTilePrefab.GetComponentInChildren<Renderer>();
        gridData.cellSize = rend != null ? rend.bounds.size.x : gridData.defaultTilePrefab.transform.localScale.x;
    }

    private void PlaceInitialTiles(GridMap map)
    {
        if (map == null || tileContainer == null || gridData.defaultTilePrefab == null) return;

        foreach (var node in map.Nodes)
        {
            GameObject tileObj = Instantiate(gridData.defaultTilePrefab, node.WorldPosition, Quaternion.identity, tileContainer);
            
            TileView tileView = tileObj.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(node, gridData.defaultTileData);
            }

            GridTile tileComp = tileObj.GetComponent<GridTile>();
            if (tileComp != null)
            {
                tileComp.Setup(gridData.defaultTileData);
                node.Tile = tileComp;
            }
        }
    }

    public void UpdateHoverView(GridNode prevNode, GridNode currNode)
    {
        if (prevNode != null && prevNode.CurrentTile != null)
            prevNode.CurrentTile.SetHighlight(false);

        if (currNode != null && currNode.CurrentTile != null)
            currNode.CurrentTile.SetHighlight(true);
    }
}