using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private UnitDataSO allyUnit; // 테스트용 아군 유닛

    private IEnumerator Start()
    {
        // 1. 시스템 초기화 대기 (Grid 생성 등)
        yield return null; 
        yield return null; // 안전하게 2프레임 대기

        // 2. 아군 유닛 무작위 배치
        PlaceRandomAlly();

        // 3. 웨이브 1 강제 시작 (인덱스 0)
        Debug.Log("[Tester] 테스트를 위해 웨이브 1을 강제로 시작합니다.");
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.StartWave(0);
        }
    }

    private void PlaceRandomAlly()
    {
        if (GridManager.Instance == null || UnitManager.Instance == null || allyUnit == null) 
        {
            Debug.LogWarning("[Tester] 매니저 또는 테스트 유닛 데이터가 없습니다.");
            return;
        }

        GridDataSO gridData = GridManager.Instance.Data;
        if (gridData == null) return;

        int maxAttempts = 100;
        GridNode targetNode = null;

        // 무작위 위치 찾기 시도
        for (int i = 0; i < maxAttempts; i++)
        {
            int x = Random.Range(0, gridData.width);
            int y = Random.Range(0, gridData.height);

            GridNode node = GridManager.Instance.GetNode(x, y);

            // 유효성 검사: 
            // 1. 노드가 존재하고
            // 2. 걷기 가능하며 (벽이 아님)
            // 3. 이미 유닛이 있지 않고
            // 4. 스폰 노드나 코어 노드가 아닌 곳
            if (node != null && node.IsWalkable)
            {
                var units = UnitManager.Instance.GetUnitsOnNode(node);
                bool isSpawnNode = (node == GridManager.Instance.GetSpawnNode());
                bool isCoreNode = (node == GridManager.Instance.GetCoreNode());

                if (units.Count == 0 && !isSpawnNode && !isCoreNode)
                {
                    targetNode = node;
                    break;
                }
            }
        }

        if (targetNode != null)
        {
            Unit unit = UnitManager.Instance.SpawnUnit(allyUnit, targetNode);
            Debug.Log($"[Tester] 아군 유닛({allyUnit.unitName}) 무작위 배치: {targetNode.Coordinate}");
        }
        else
        {
            Debug.LogWarning("[Tester] 유닛을 배치할 빈 공간을 찾지 못했습니다.");
        }
    }
}