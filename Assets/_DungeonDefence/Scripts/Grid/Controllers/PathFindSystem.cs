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

    // 현재 위치에서 이동할 가장 매력도가 높은 이웃 좌표를 반환합니다.
    public Vector2Int GetNextStep(Vector2Int currentPos)
    {
        if (map == null) return currentPos;

        GridNode currentNode = map.GetNode(currentPos.x, currentPos.y);
        if (currentNode == null) return currentPos;
        List<GridNode> neighbors = GetNeighbors(currentNode);

        // 이동 불가능한 곳 제외
        // (기존 로직에 왔던 길을 되돌아가지 않는 조건이 있었다면 여기에 visited 체크 추가 필요)
        List<GridNode> candidates = new List<GridNode>();
        foreach (var node in neighbors)
        {
            if (node.IsWalkable) // GridNode에 추가한 IsWalkable 프로퍼티 사용
            {
                candidates.Add(node);
            }
        }

        if (candidates.Count == 0) return currentPos; // 갈 곳이 없음

        // 3. 매력도가 가장 높은 타일 찾기 (내림차순 정렬)
        // 만약 매력도가 같다면? -> 거리나 무작위 등 '우선순위 룰'이 필요할 수 있음
        // 여기서는 단순히 먼저 발견된 순서 혹은 리스트 정렬 순서를 따름
        candidates.Sort((a, b) => b.Attractiveness.CompareTo(a.Attractiveness));

        return candidates[0].Coordinate;
    }

    private List<GridNode> GetNeighbors(GridNode node)
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