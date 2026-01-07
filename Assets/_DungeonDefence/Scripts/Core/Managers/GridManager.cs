using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
    맵 크기와 노드, 타일을 관리하는 매니저
*/

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 3;
    [SerializeField] private int height = 5;
    [SerializeField] private float cellSize = 1.0f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Initial Tiles")]
    public (int x, int y) spawnNodePos = (0, 0);
    public (int x, int y) coreNodePos = (2, 4);

    [Header("Temp Setting")]
    [SerializeField] private GameObject defaultTilePrefab;
    [SerializeField] private TileData defaultTileData;

    private Node[,] grid;
    private Node spawnNode;
    private Node coreNode;

    void Awake()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        grid = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(x * cellSize, 0, y * cellSize);
                grid[x, y] = new Node(x, y, worldPos);

                if (x == spawnNodePos.x && y == spawnNodePos.y) spawnNode = grid[x, y];
                if (x == coreNodePos.x && y == coreNodePos.y)coreNode = grid[x, y];

                PlaceTile(grid[x, y], defaultTilePrefab);
            }
        }          
    }

    public Node GetNode(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x + cellSize/2) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.z + cellSize/2) / cellSize);
        
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return grid[x, y];
        }
        return null;
    }

    public void PlaceTile(Node node, GameObject TilePrefab)
    {
        Debug.Log("Trying to place tile");
        if (node == null || node.CurrentTile != null) return;
        
        GameObject tile = Instantiate(TilePrefab, node.WorldPosition, Quaternion.identity, transform);
        Tile tileComponent = tile.GetComponent<Tile>();

        tileComponent.Setup(node, defaultTileData);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(x * cellSize, 0, y * cellSize);
                Gizmos.DrawWireCube(worldPos, new Vector3(cellSize, 0.1f, cellSize));
            }
        }        
    }
}