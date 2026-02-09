using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinderSystem
{
    private GridMap map;

    public void Initialize(GridMap map)
    {
        this.map = map;
    }

    public Vector2Int GetNextStep(Vector2Int currentPos)
    {
        if (map == null) return currentPos;

        GridNode currentNode = map.GetNode(currentPos.x, currentPos.y);
        if (currentNode == null) return currentPos;
        List<GridNode> neighbors = GetNeighbors(currentNode);

        List<GridNode> candidates = new List<GridNode>();
        foreach (var node in neighbors)
        {
            candidates.Add(node);
        }

        if (candidates.Count == 0) return currentPos;
        candidates.Sort((a, b) => b.Attractiveness.CompareTo(a.Attractiveness));

        return candidates[0].Coordinate;
    }

    public List<GridNode> GetNeighbors(GridNode node)
    {
        List<GridNode> results = new List<GridNode>();
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = node.X + dx[i];
            int ny = node.Y + dy[i];

            if (map.IsValid(nx, ny))
            {
                results.Add(map.Nodes[nx, ny]);
            }
        }
        return results;
    }
}