using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinder : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private int visitedPenalty = 50;

    private Unit _unit;
    private HashSet<GridNode> _visitedNodes = new HashSet<GridNode>();

    public void Initialize(Unit unit)
    {
        _unit = unit;
        _visitedNodes.Clear();
        if (_unit.CurrentNode != null) _visitedNodes.Add(_unit.CurrentNode);
    }

    public void RecordVisit(GridNode node)
    {
        if (node != null) _visitedNodes.Add(node);
    }

    public GridNode GetNextNode()
    {
        GridNode current = _unit.CurrentNode;
        if (current == null) return null;

        GridNode bestNode = null;
        int maxScore = int.MinValue;

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            GridNode neighbor = GridManager.Instance.GetNode(current.X + dx[i], current.Y + dy[i]);

            if (neighbor == null || neighbor.Attractive <= -9000) continue;

            int score = neighbor.Attractive;
            if (_visitedNodes.Contains(neighbor)) score -= visitedPenalty;

            if (score > maxScore)
            {
                maxScore = score;
                bestNode = neighbor;
            }
        }

        return bestNode;
    }
}