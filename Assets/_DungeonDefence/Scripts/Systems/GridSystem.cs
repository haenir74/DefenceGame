using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem
{
    private PathFinder pathFinder;

    public GridSystem()
    {
        pathFinder = new PathFinder(this);
    }

    public void Generate(MapContext map, GridData data)
    {
        map.Initialize(data.width, data.height);

        for (int x = 0; x < data.width; x++)
        {
            for (int y = 0; y < data.height; y++)
            {
                Vector3 worldPos = GetWorldPosition(x, y, data.cellSize);
                Node newNode = new Node(x, y, worldPos);
                
                map.Nodes[x, y] = newNode;

                if (x == data.spawnNodePos.x && y == data.spawnNodePos.y)
                    map.SpawnNode = newNode;
                if (x == data.coreNodePos.x && y == data.coreNodePos.y)
                    map.CoreNode = newNode;
            }
        }
    }

    public Node GetNode(MapContext map, GridData data, Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x + data.cellSize * 0.5f) / data.cellSize);
        int y = Mathf.FloorToInt((worldPosition.z + data.cellSize * 0.5f) / data.cellSize);

        if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
        {
            return map.Nodes[x, y];
        }
        return null;
    }
    
    public bool IsValid(MapContext map, int x, int y)
    {
        return x >= 0 && x < map.Width && y >= 0 && y < map.Height;
    }

    public Vector3 GetWorldPosition(int x, int y, float cellSize)
    {
        return new Vector3(x * cellSize, 0, y * cellSize);
    }

    public List<Node> FindPath(MapContext map, Node startNode, Node targetNode)
    {
        return pathFinder.FindPath(map, startNode, targetNode);
    }
}
