using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("테스트 시작 전 대기 시간 (초)")]
    [SerializeField] private float startDelay = 1.0f;

    [Header("Unit Test")]
    [Tooltip("배치할 아군 유닛 데이터")]
    [SerializeField] private UnitDataSO allyUnitData;
    [Tooltip("배치할 아군 유닛 수")]
    [SerializeField] private int allyCount = 3;

    [Header("Tile Test")]
    [Tooltip("경로 유도 테스트용 '길(Road)' 타일 데이터")]
    [SerializeField] private TileDataSO roadTileData;
    [Tooltip("길 타일을 배치할지 여부")]
    [SerializeField] private bool placeTestRoad = true;

    private GridController gridController;

    private IEnumerator Start()
    {
        // 1. 초기화 대기
        yield return new WaitForSeconds(startDelay);

        if (!CheckManagers())
        {
            Debug.LogError("[Tester] 필수 매니저가 초기화되지 않았습니다.");
            yield break;
        }

        gridController = FindObjectOfType<GridController>();

        // 2. 맵 전체를 훑는 경로 생성 (Extreme Snake Path)
        if (placeTestRoad && roadTileData != null)
        {
            Debug.Log("[Tester] 맵 전체를 횡단하는 긴 경로(Extreme Snake)를 생성합니다.");
            PlaceExtremeSnakePath();
        }

        // 3. 아군 유닛 길 위에 배치
        if (allyUnitData != null)
        {
            Debug.Log($"[Tester] 아군 유닛 {allyCount}기를 길 위에 배치합니다.");
            PlaceAlliesOnRoad();
        }

        // 4. 웨이브 1 강제 시작
        Debug.Log("[Tester] 웨이브 1을 시작합니다.");
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.StartWave(0);
        }
    }

    private void PlaceExtremeSnakePath()
    {
        GridDataSO gridData = GridManager.Instance.Data;
        if (gridData == null || gridController == null) return;

        // (2,0) -> (2,7) 기준 로직
        int width = gridData.width;   // 5
        int height = gridData.height; // 8
        
        // 시작점(2,0)에서 왼쪽 벽(0,0)으로 이동 후 시작
        // 경로: (2,0) -> (1,0) -> (0,0) -> (0,1) -> (4,1) -> (4,2) -> (0,2) ... 
        
        // 0행: (2,0) -> (0,0) 쪽으로 연결
        ChangeTileAt(2, 0); // Spawn
        ChangeTileAt(1, 0);
        ChangeTileAt(0, 0);

        // 1행부터 7행(코어 전)까지 지그재그
        for (int y = 1; y < height; y++)
        {
            // 코어가 있는 마지막 행(7행) 처리
            if (y == gridData.coreNodePos.y)
            {
                // 홀수 행에서 올라왔다면 x=0, 짝수 행에서 올라왔다면 x=4에 있을 것임.
                // 현재 예시: 
                // y=0: 2->0 (Left)
                // y=1: 0->4 (Right)
                // y=2: 4->0 (Left)
                // ...
                // y=6: 4->0 (Left)
                // y=7: 0->2 (Right to Core)
                
                // 마지막 행은 현재 위치에서 코어(2,7)까지만 연결
                int startX = (y % 2 != 0) ? 0 : width - 1; // 로직에 따라 달라질 수 있음, 아래 루프에 맞춤
                
                // 실제 y-1행의 끝지점 확인 필요.
                // y=1 (Right): 끝이 4
                // y=2 (Left): 끝이 0
                // y=6 (Left): 끝이 0
                // 따라서 y=7은 0에서 시작해서 2로 가야 함.
                
                // 일반화: 이전 행의 방향에 따라 시작점 결정
                bool prevWasRight = ((y - 1) % 2 != 0); 
                int currentX = prevWasRight ? width - 1 : 0;
                int coreX = gridData.coreNodePos.x;

                // 현재 x에서 코어 x까지 이동
                int step = (currentX < coreX) ? 1 : -1;
                while (currentX != coreX)
                {
                    ChangeTileAt(currentX, y);
                    currentX += step;
                }
                ChangeTileAt(coreX, y); // Core
                break;
            }

            // 일반 행 (1 ~ 6)
            if (y % 2 != 0) // 홀수 행: 왼쪽(0) -> 오른쪽(4)
            {
                // 이전 행(0,0)에서 올라왔으므로 (0,y)부터 시작
                for (int x = 0; x < width; x++) ChangeTileAt(x, y);
            }
            else // 짝수 행: 오른쪽(4) -> 왼쪽(0)
            {
                // 이전 행(4,y-1)에서 올라왔으므로 (4,y)부터 시작
                for (int x = width - 1; x >= 0; x--) ChangeTileAt(x, y);
            }
        }
    }

    private void ChangeTileAt(int x, int y)
    {
        GridNode node = GridManager.Instance.GetNode(x, y);
        // 시각적 구분을 위해 스폰/코어 노드는 타일 변경 제외 (옵션)
        if (node != null)
        {
            // 스폰(2,0)과 코어(2,7)는 색을 바꾸지 않더라도, 
            // 실제 이동 로직상으로는 'Road'의 높은 점수가 필요할 수 있음.
            // 하지만 테스트를 위해 놔두거나, 혹은 타일 데이터만 바꾸고 비주얼은 유지할 수도 있음.
            // 여기서는 비주얼까지 다 바꿉니다.
            if (node == GridManager.Instance.GetSpawnNode()) return;
            if (node == GridManager.Instance.GetCoreNode()) return;

            gridController.ChangeTile(node, roadTileData);
        }
    }

    private void PlaceAlliesOnRoad()
    {
        GridDataSO gridData = GridManager.Instance.Data;
        int placedCount = 0;
        int maxAttempts = 200;

        for (int i = 0; i < maxAttempts; i++)
        {
            if (placedCount >= allyCount) break;

            int x = Random.Range(0, gridData.width);
            int y = Random.Range(0, gridData.height);
            GridNode node = GridManager.Instance.GetNode(x, y);

            // 1. 유효한 노드인지
            if (node == null) continue;
            // 2. Road 타일인지 (빨간색 타일 위에만 배치)
            if (node.Tile == null || node.Tile.Data != roadTileData) continue;
            // 3. 이미 유닛이 있는지
            if (UnitManager.Instance.GetUnitsOnNode(node).Count > 0) continue;
            // 4. 스폰/코어 제외
            if (node == GridManager.Instance.GetSpawnNode() || node == GridManager.Instance.GetCoreNode()) continue;

            UnitManager.Instance.SpawnUnit(allyUnitData, node);
            placedCount++;
        }
    }

    private bool CheckManagers()
    {
        return GridManager.Instance != null && 
               UnitManager.Instance != null && 
               WaveManager.Instance != null;
    }
}