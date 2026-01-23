using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridData", menuName = "Game/Grid/GridData")]
public class GridData : ScriptableObject
{
    [Header("Settings")]
    public int width = 5;
    public int height = 8;
    public float cellSize = 1.0f; 

    public LayerMask groundLayer;

    [Header("Key Nodes")]
    public Vector2Int spawnNodePos = new Vector2Int(2, 0);
    public Vector2Int coreNodePos = new Vector2Int(2, 7);

    [Header("Default Datas")]
    public GameObject defaultTilePrefab;
    public GameObject coreUnitPrefab;
    public TileData defaultTileData;
}
