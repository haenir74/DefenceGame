using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathFinder : MonoBehaviour
{
    [SerializeField] private int visitedPenalty = 1000;

    private Unit unit;
    private Vector2Int? currentTarget;
    private List<Vector2Int> visitedNodes = new List<Vector2Int>();

    private GridNode coreNode;

    private void Awake()
    {
        this.unit = GetComponent<Unit>();
    }

    public void Initialize(GridNode startNode)
    {
        visitedNodes.Clear();
        if (startNode != null)
        {
            visitedNodes.Add(startNode.Coordinate);
        }
        currentTarget = null;
        coreNode = GridManager.Instance.GetCoreNode();
    }

    public void FindNextStep()
    {
        if (unit == null || unit.CurrentNode == null || coreNode == null) return;

        List<GridNode> neighbors = GridManager.Instance.GetNeighbors(unit.CurrentNode);
        
        GridNode bestNode = null;
        int maxScore = int.MinValue;

        foreach (var node in neighbors)
        {
            int score = CalculateScore(node);
            if (score > maxScore)
            {
                maxScore = score;
                bestNode = node;
            }
        }

        if (bestNode != null)
        {
            currentTarget = bestNode.Coordinate;
        }
        else
        {
            currentTarget = null;
        }
    }

    private int CalculateScore(GridNode node)
    {
        int dist = node.GetDistance(coreNode);
        int distanceScore = 10000 - (dist * 10); 
        int tileBonus = node.Attractiveness;
        int visitCount = 0;
        foreach (var visited in visitedNodes)
        {
            if (visited == node.Coordinate) visitCount++;
        }
        int penalty = visitCount * visitedPenalty;

        return distanceScore + tileBonus - penalty;
    }

    public Vector2Int? GetTargetStep()
    {
        if (currentTarget == null) FindNextStep();
        return currentTarget;
    }

    public void OnMoveCompleted(GridNode node)
    {
        currentTarget = null;
        if (node != null)
        {
            visitedNodes.Add(node.Coordinate);
        }
    }
}