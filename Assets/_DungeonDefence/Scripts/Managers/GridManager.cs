using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private GridData gridData;
    [SerializeField] private GridView gridView;
    [SerializeField] private Transform TileCotntainer;

    private GridSystem gridSystem;

    private MapContext Map => GameManager.Instance.Context.map;
    public GridData Data => gridData;

    void Awake()
    {
        Instance = this;
        gridSystem = new GridSystem();
    }

    void Start()
    {
        gridSystem.Generate(Map, gridData);
        if (gridView != null) gridView.Setup(Map, gridData);
        PlaceInitialTiles();
    }

    private void PlaceInitialTiles()
    {
        foreach (var node in Map.Nodes)
        {
            GameObject tileObj = Instantiate(gridData.defaultTilePrefab, node.WorldPosition, Quaternion.identity, TileCotntainer);
            TileView tileView = tileObj.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(node, gridData.defaultTileData);
            }
        }
    }

    public Node GetNode(Vector3 worldPos)
    {
        return gridSystem.GetNode(Map, gridData, worldPos);
    }

    public void OnHoverChanged(Node prevNode, Node currNode)
    {
        if (prevNode != null && prevNode.CurrentTile != null)
            prevNode.CurrentTile.SetHighlight(false);

        if (currNode != null && currNode.CurrentTile != null)
            currNode.CurrentTile.SetHighlight(true);
    }
}