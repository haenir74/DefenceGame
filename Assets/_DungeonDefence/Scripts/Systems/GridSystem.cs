using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridSystem
{
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

    #region Pathfinding (A*)

    public List<Node> FindPath(MapContext map, Node startNode, Node targetNode)
    {
        if (startNode == null || targetNode == null) return null;

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        
        openSet.Add(startNode);

        // A*를 위해 Node 클래스에 G, H, Parent 등을 저장하면 좋지만, 
        // Node 클래스를 수정하지 않고 외부 Dictionary를 사용해 관리합니다.
        Dictionary<Node, Node> parentMap = new Dictionary<Node, Node>();
        Dictionary<Node, float> gScore = new Dictionary<Node, float>();
        Dictionary<Node, float> fScore = new Dictionary<Node, float>();

        gScore[startNode] = 0;
        fScore[startNode] = GetDistance(startNode, targetNode);

        while (openSet.Count > 0)
        {
            Node current = openSet.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : float.MaxValue).First();

            if (current == targetNode)
            {
                return RetracePath(parentMap, startNode, targetNode);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Node neighbor in GetNeighbors(map, current))
            {
                // 이동 불가능한 타일 체크 (예: 벽) - 현재는 모든 타일 이동 가능으로 가정
                // if (!neighbor.IsWalkable) continue; 
                if (closedSet.Contains(neighbor)) continue;

                float tentativeGScore = gScore[current] + GetDistance(current, neighbor);

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    parentMap[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + GetDistance(neighbor, targetNode);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // 경로 없음
    }

    private List<Node> RetracePath(Dictionary<Node, Node> parentMap, Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            if (parentMap.ContainsKey(currentNode))
                currentNode = parentMap[currentNode];
            else
                break; // Should not happen if path exists
        }
        
        path.Reverse();
        return path;
    }

    private List<Node> GetNeighbors(MapContext map, Node node)
    {
        List<Node> neighbors = new List<Node>();

        int[] xDirs = { 0, 0, 1, -1 };
        int[] yDirs = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = node.X + xDirs[i];
            int checkY = node.Y + yDirs[i];

            if (IsValid(map, checkX, checkY))
            {
                neighbors.Add(map.Nodes[checkX, checkY]);
            }
        }

        return neighbors;
    }

    private float GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.X - nodeB.X);
        int dstY = Mathf.Abs(nodeA.Y - nodeB.Y);
        return dstX + dstY; // Manhattan distance for grid
    }

    #endregion
}
