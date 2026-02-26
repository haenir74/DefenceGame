using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridData", menuName = "DungeonDefence/Datas/GridData")]
public class GridDataSO : ScriptableObject
{
    
    public int width = 5;
    public int height = 8;
    public float cellSize = 1.0f; 

    public LayerMask groundLayer;

    
    public Vector2Int spawnNodePos = new Vector2Int(2, 0);
    public Vector2Int coreNodePos = new Vector2Int(2, 7);

    
    public GameObject defaultTilePrefab;
    public GameObject coreUnitPrefab;
    public TileDataSO defaultTileData;
}



