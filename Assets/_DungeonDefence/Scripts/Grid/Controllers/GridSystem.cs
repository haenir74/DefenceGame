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

    public List<GridNode> GetNeighbors(GridMap map, GridNode node, bool includeDiagonals = false)
    {
        List<GridNode> neighbors = new List<GridNode>();
        if (map == null || node == null) return neighbors;

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = node.X + dx[i];
            int ny = node.Y + dy[i];

            if (map.IsValid(nx, ny))
            {
                neighbors.Add(map.Nodes[nx, ny]);
            }
        }
        return neighbors;
    }

    public void CalculateFlowField(GridMap map, GridNode coreNode)
    {
        if (map == null || coreNode == null) return;

        foreach (var node in map.Nodes)
        {
            node.DistanceToCore = int.MaxValue;
        }

        Queue<GridNode> queue = new Queue<GridNode>();
        coreNode.DistanceToCore = 0;
        queue.Enqueue(coreNode);

        while (queue.Count > 0)
        {
            GridNode current = queue.Dequeue();
            int currentDist = current.DistanceToCore;

            List<GridNode> neighbors = GetNeighbors(map, current);
            foreach (var neighbor in neighbors)
            {
                if (neighbor.DistanceToCore > currentDist + 1)
                {
                    neighbor.DistanceToCore = currentDist + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    public void CalculateAttractivenessInfluence(GridMap map)
    {
        if (map == null) return;


        foreach (var node in map.Nodes)
        {
            node.TileInfluence = 0;
        }

        int maxRange = 5;
        float decayFactor = 0.5f;
        float radiationMultiplier = 0.5f;


        foreach (var sourceNode in map.Nodes)
        {
            int bonus = sourceNode.GetTileBonus();
            if (bonus == 0) continue;



            Queue<(GridNode node, int dist)> queue = new Queue<(GridNode, int)>();
            HashSet<GridNode> visited = new HashSet<GridNode>();

            queue.Enqueue((sourceNode, 0));
            visited.Add(sourceNode);

            while (queue.Count > 0)
            {
                var (current, dist) = queue.Dequeue();


                if (dist > 0)
                {
                    float decay = Mathf.Pow(decayFactor, dist);
                    current.TileInfluence += Mathf.RoundToInt(bonus * radiationMultiplier * decay);
                }

                if (dist < maxRange)
                {
                    foreach (var neighbor in GetNeighbors(map, current))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue((neighbor, dist + 1));
                        }
                    }
                }
            }
        }
    }
}



