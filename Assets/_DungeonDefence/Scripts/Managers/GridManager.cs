using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [SerializeField] private GridData gridData;
    [SerializeField] private GridView gridView;
    [SerializeField] private Transform tileContainer;

    private GridSystem gridSystem;
    public GridSystem GridSystem => gridSystem;
    public GridData Data => gridData;

    private MapContext Map => GameManager.Instance != null ? GameManager.Instance.Context.map : null;

    protected override void Awake()
    {
        base.Awake();
        gridSystem = new GridSystem();
    }

    private void Start()
    {
        if (Map == null) return;

        CalculateCellSize();
        gridSystem.Generate(Map, gridData);
        if (gridView != null) gridView.Setup(Map, gridData);
        
        PlaceInitialTiles();
        PlaceCoreUnit();

        SetupCamera();
    }

    private void CalculateCellSize()
    {
        if (gridData.defaultTilePrefab == null)
        {
            Debug.LogError("Default Tile Prefab is missing in GridData!");
            return;
        }

        Renderer rend = gridData.defaultTilePrefab.GetComponentInChildren<Renderer>();
        if (rend != null)
        {

            gridData.cellSize = rend.bounds.size.x;
            Debug.Log($"Auto-calculated Cell Size: {gridData.cellSize}");
        }
        else
        {
            
            gridData.cellSize = gridData.defaultTilePrefab.transform.localScale.x;
            Debug.LogWarning("No Renderer found on Tile Prefab. Using Transform Scale X as Cell Size.");
        }
    }

    private void PlaceInitialTiles()
    {
        if (Map == null || tileContainer == null) return;

        foreach (var node in Map.Nodes)
        {
            if (gridData.defaultTilePrefab == null) continue;

            GameObject tileObj = Instantiate(gridData.defaultTilePrefab, node.WorldPosition, Quaternion.identity, tileContainer);

            TileView tileView = tileObj.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(node, gridData.defaultTileData);
            }
        }
    }

    private void PlaceCoreUnit()
    {
        if (gridData.coreUnitPrefab == null) return;
        if (Map?.CoreNode == null) return;

        GameObject coreObj = Instantiate(gridData.coreUnitPrefab, Map.CoreNode.WorldPosition, Quaternion.identity);
        Unit coreUnit = coreObj.GetComponent<Unit>();
        
        if (coreUnit != null)
        {
            coreUnit.SetNode(Map.CoreNode);
            // 필요하다면 초기화 데이터 주입
            // coreUnit.InitializeUnit(coreUnitData); 
        }
    }

    private void SetupCamera()
    {
        var camCtrl = CameraController.Instance;
        if (camCtrl != null)
        {
            camCtrl.SetupCamera(gridData.width, gridData.height, gridData.cellSize);
        }
    }

    public Node GetNode(Vector3 worldPos)
    {
        return Map != null ? gridSystem.GetNode(Map, gridData, worldPos) : null;
    }

    public void OnHoverChanged(Node prevNode, Node currNode)
    {
        if (prevNode != null && prevNode.CurrentTile != null)
            prevNode.CurrentTile.SetHighlight(false);

        if (currNode != null && currNode.CurrentTile != null)
            currNode.CurrentTile.SetHighlight(true);
    }
}