using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinder : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int coreBaseScore = 500;
    [SerializeField] private int distancePenalty = 5;
    [SerializeField] private int visitedPenalty = 200;

    private Unit _unit;
    private Dictionary<GridNode, int> _visited = new Dictionary<GridNode, int>();

    public void Initialize(Unit unit)
    {
        _unit = unit;
        _visited.Clear();
    }

    public void RecordVisit(GridNode node)
    {
        if (node == null) return;
        if (_visited.ContainsKey(node)) _visited[node]++;
        else _visited[node] = 1;
    }

    public GridNode GetNextNode()
    {
        GridNode current = _unit.CurrentNode;
        GridNode core = GridManager.Instance.GetCoreNode();
        if (current == null || core == null) return null;

        RecordVisit(current);

        GridNode bestNode = null;
        int maxScore = int.MinValue;

        List<GridNode> neighbors = GridManager.Instance.GetNeighbors(current);

        foreach (GridNode neighbor in neighbors)
        {
            int tileBonus = neighbor.GetTileBonus();
            
            int dist = neighbor.GetDistance(core); 
            int distScore = coreBaseScore - (dist * distancePenalty);

            int penalty = 0;
            if (_visited.TryGetValue(neighbor, out int count))
            {
                penalty = count * visitedPenalty;
            }
            int totalScore = tileBonus + distScore - penalty;

            if (totalScore > maxScore)
            {
                maxScore = totalScore;
                bestNode = neighbor;
            }
            else if (totalScore == maxScore)
            {
                if (bestNode != null && dist < bestNode.GetDistance(core))
                {
                    bestNode = neighbor;
                }
            }
        }
        return bestNode;
    }
}