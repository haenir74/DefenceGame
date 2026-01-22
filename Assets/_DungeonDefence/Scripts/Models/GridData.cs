using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridData", menuName = "Game/Grid/GridData")]
public class GridData : ScriptableObject
{
    [Header("Settings")]
    public int width = 3;
    public int height = 5;
    public float cellSize = 1.0f;
    public LayerMask groundLayer;

    [Header("Key Nodes")]
    public Vector2Int spawnNodePos = new Vector2Int(0, 0);
    public Vector2Int coreNodePos = new Vector2Int(2, 4);

    [Header("Prefabs")]
    public GameObject defaultTilePrefab;
    public TileData defaultTileData;
}