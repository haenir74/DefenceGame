using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem
{
    public void Generate(GridMap map, GridDataSO data)
    {
        map.Initialize(data.width, data.height);

        for (int x = 0; x < data.width; x++)
        {
            for (int y = 0; y < data.height; y++)
            {
                Vector3 worldPos = GetWorldPosition(x, y, data.cellSize);
                GridNode newNode = new GridNode(x, y, worldPos);
                map.Nodes[x, y] = newNode;

                if (x == data.spawnNodePos.x && y == data.spawnNodePos.y)
                    map.SpawnNode = newNode;
                if (x == data.coreNodePos.x && y == data.coreNodePos.y)
                    map.CoreNode = newNode;
            }
        }
    }

    private GridNode GetNode(GridMap map, GridDataSO data, int x, int y)
    {
        if (map == null || !map.IsValid(x, y)) return null;
        return map.Nodes[x, y];
    }

    public GridNode GetNode(GridMap map, GridDataSO data, Vector3 worldPosition)
    {
        if (map == null || data == null) return null;
        
        float halfCell = data.cellSize * 0.5f;
        int x = Mathf.FloorToInt((worldPosition.x + halfCell) / data.cellSize);
        int y = Mathf.FloorToInt((worldPosition.z + halfCell) / data.cellSize);
        
        return map.GetNode(x, y);
    }

    public Vector3 GetWorldPosition(int x, int y, float cellSize)
    {
        return new Vector3(x * cellSize, 0, y * cellSize);
    }
}

