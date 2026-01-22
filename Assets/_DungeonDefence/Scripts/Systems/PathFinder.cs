using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathFinder
{
    private GridSystem gridSystem;

    public PathFinder(GridSystem gridSystem)
    {
        this.gridSystem = gridSystem;
    }

    public List<Node> FindPath(MapContext map, Node startNode, Node targetNode)
    {
        if (startNode == null || targetNode == null) return null;

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        
        openSet.Add(startNode);

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

        return null;
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
                break;
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

            if (gridSystem.IsValid(map, checkX, checkY))
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
        return dstX + dstY;
    }
}
