using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem
{
    private const float STANCE_OFFSET_RATIO = 0.2f;

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

    public Vector3 GetEdgePosition(Node a, Node b)
    {
        return (a.WorldPosition + b.WorldPosition) * 0.5f;
    }

    public Vector3 GetStancePosition(Node node, Team team, Vector3 flowDirection, float cellSize)
    {
        float offsetFactor = (team == Team.Enemy) ? STANCE_OFFSET_RATIO : -STANCE_OFFSET_RATIO;
        return node.WorldPosition + (flowDirection * offsetFactor * cellSize);
    }

    public Vector3 GetPlacementPosition(Node currentNode, Node targetNode, Team team, float cellSize)
    {
        if (targetNode == null || currentNode == targetNode) 
            return currentNode.WorldPosition;
        Vector3 flowDir = (targetNode.WorldPosition - currentNode.WorldPosition).normalized;
        return GetStancePosition(currentNode, team, flowDir, cellSize);
    }

    public List<Node> GetNeighbors(MapContext map, Node node)
    {
        List<Node> neighbors = new List<Node>();
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = node.X + dx[i];
            int ny = node.Y + dy[i];
            Node neighbor = map.GetNode(nx, ny);
            if (neighbor != null) neighbors.Add(neighbor);
        }
        return neighbors;
    }
}

