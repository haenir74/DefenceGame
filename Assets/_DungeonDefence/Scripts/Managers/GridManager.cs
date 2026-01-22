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
        CalculateCellSize();
        gridSystem.Generate(Map, gridData);
        
        if (gridView != null) gridView.Setup(Map, gridData);
        
        PlaceInitialTiles();
        
        // 카메라 설정 위임
        if (CameraController.Instance != null)
        {
            CameraController.Instance.SetupCamera(gridData.width, gridData.height, gridData.cellSize);
        }
        else
        {
            // 씬에 CameraController가 없을 경우 동적으로 찾거나 경고
            var camCtrl = FindObjectOfType<CameraController>();
            if (camCtrl != null)
            {
                camCtrl.SetupCamera(gridData.width, gridData.height, gridData.cellSize);
            }
        }
    }

    private void CalculateCellSize()
    {
        if (gridData.defaultTilePrefab == null)
        {
            Debug.LogError("Default Tile Prefab is missing in GridData!");
            return;
        }

        // 프리팹의 렌더러를 찾아 실제 크기(Bounds)를 측정합니다.
        Renderer rend = gridData.defaultTilePrefab.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            // X축 크기를 기준으로 정사각형 셀 크기를 정합니다.
            // 프리팹의 Scale이 적용된 최종 월드 크기가 반환됩니다.
            gridData.cellSize = rend.bounds.size.x;
            Debug.Log($"Auto-calculated Cell Size: {gridData.cellSize}");
        }
        else
        {
            // 렌더러가 없으면 기본값 1 또는 프리팹의 Scale X를 사용
            gridData.cellSize = gridData.defaultTilePrefab.transform.localScale.x;
            Debug.LogWarning("No Renderer found on Tile Prefab. Using Transform Scale X as Cell Size.");
        }
    }

    private void PlaceInitialTiles()
    {
        foreach (var node in Map.Nodes)
        {
            GameObject tileObj = Instantiate(gridData.defaultTilePrefab, node.WorldPosition, Quaternion.identity, TileCotntainer);
            
            // 프리팹의 크기가 변경되었어도, 위치는 이미 cellSize에 맞춰 GridSystem에서 계산됨.
            
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