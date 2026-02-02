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

        if (_visited.ContainsKey(node))
        {
            _visited[node]++;
        }
        else
        {
            _visited[node] = 1;
        }
    }

    public GridNode GetNextNode()
    {
        GridNode current = _unit.CurrentNode;
        GridNode core = GridManager.Instance.Map.CoreNode;
        if (current == null || core == null) return null;

        RecordVisit(current);

        GridNode bestNode = null;
        int maxScore = int.MinValue;

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            GridNode neighbor = GridManager.Instance.GetNode(current.X + dx[i], current.Y + dy[i]);

            if (neighbor == null) continue;
            // if (neighbor.IsWall) continue; // 벽 기능 구현 시 추가
            int tileBonus = neighbor.GetTileBonus();

            int dist = neighbor.GetDistance(core);
            int distScore = coreBaseScore - (dist * distancePenalty);

            int penalty = 0;
            if (_visited.TryGetValue(neighbor, out int count))
            {
                penalty = count * visitedPenalty;
            }
            int totalScore = tileBonus + distScore - penalty;

            // --- 디버깅용 로그 (필요시 사용) ---
            Debug.Log($"Node({neighbor.X},{neighbor.Y}) : Tile({tileBonus}) + Dist({distScore}) - Visited({penalty}) = {totalScore}");

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