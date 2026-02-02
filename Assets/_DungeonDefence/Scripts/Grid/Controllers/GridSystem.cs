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

    public void CalculateAttractiveness(GridMap map, GridDataSO data)
    {
        if (map.CoreNode == null) return;

        foreach (var node in map.Nodes)
        {
            node.Attractive = -9999;
        }

        // BFS 탐색
        Queue<GridNode> queue = new Queue<GridNode>();

        map.CoreNode.Attractive = 1000; 
        queue.Enqueue(map.CoreNode);

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        while (queue.Count > 0)
        {
            GridNode current = queue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                GridNode neighbor = GetNode(map, data, current.X + dx[i], current.Y + dy[i]);
                if (neighbor == null || neighbor.Attractive != -9999) continue;
                // if (!neighbor.IsWalkable) continue;
                neighbor.Attractive = current.Attractive - 1;
                
                queue.Enqueue(neighbor);
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

