using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 테스트 전용 디버그 매니저.
/// Inspector에서 설정한 유닛/웨이브 데이터를 이용해 인벤토리 없이 바로 테스트할 수 있게 해줌.
/// 이 컴포넌트를 씬의 아무 오브젝트에나 붙이면 됨.
/// </summary>
public class TestManager : MonoBehaviour
{
    [Header("Test Unit Settings")]
    [Tooltip("배치할 테스트 아군 유닛 데이터 (Player_TestGuardian_Data)")]
    [SerializeField] private UnitDataSO testUnitData;

    [Header("Test Wave Settings")]
    [Tooltip("시작할 테스트 웨이브 데이터 (WaveData_Test_AllEnemies)")]
    [SerializeField] private WaveDataSO testWaveData;
    [Tooltip("테스트 웨이브 시작 시 배틀 페이즈로 전환 여부")]
    [SerializeField] private bool switchToBattleOnTestWave = true;

    [Header("Auto Placement")]
    [Tooltip("게임 시작 시 자동으로 테스트 유닛을 배치할 좌표 목록")]
    [SerializeField] private List<Vector2Int> autoPlaceCoords = new List<Vector2Int>();

    private Coroutine testWaveCoroutine;

    private void Start()
    {
        if (autoPlaceCoords.Count > 0)
        {
            StartCoroutine(AutoPlaceCo());
        }
    }

    private IEnumerator AutoPlaceCo()
    {
        // GridManager 초기화 대기
        yield return new WaitUntil(() => GridManager.Instance != null && GridManager.Instance.GetCoreNode() != null);
        yield return null;

        foreach (var coord in autoPlaceCoords)
        {
            PlaceTestUnit(coord.x, coord.y);
        }
    }

    // ===== Editor Button Methods =====

    /// <summary>
    /// 지정 좌표에 테스트 아군 유닛을 즉시 스폰 (인벤토리 소비 없음).
    /// </summary>
    [ContextMenu("Spawn Test Unit at (0,0)")]
    public void SpawnTestUnitAtOrigin()
    {
        PlaceTestUnit(0, 0);
    }

    public void PlaceTestUnit(int x, int y)
    {
        if (testUnitData == null)
        {
            Debug.LogWarning("[TestManager] testUnitData가 null입니다. Inspector에서 Player_TestGuardian_Data를 연결해주세요.");
            return;
        }

        GridNode node = GridManager.Instance?.GetNode(x, y);
        if (node == null)
        {
            Debug.LogWarning($"[TestManager] 노드 ({x},{y})를 찾을 수 없습니다.");
            return;
        }

        Unit spawned = UnitManager.Instance.SpawnUnit(testUnitData, node);
        if (spawned != null)
        {
            Debug.Log($"<color=cyan>[TestManager] 테스트 유닛 배치 완료: ({x},{y})</color>");
        }
    }

    /// <summary>
    /// 테스트 웨이브를 직접 실행. WaveManager의 waves 리스트와 무관하게 동작.
    /// </summary>
    [ContextMenu("Start Test Wave")]
    public void StartTestWave()
    {
        if (testWaveData == null)
        {
            Debug.LogWarning("[TestManager] testWaveData가 null입니다. Inspector에서 WaveData_Test_AllEnemies를 연결해주세요.");
            return;
        }

        if (switchToBattleOnTestWave && GameManager.Instance != null && GameManager.Instance.IsMaintenancePhase)
        {
            // 배틀 UI로 전환 (WaveManager 스킵하고 직접 스폰)
            UIManager.Instance?.SwitchToBattlePhase();
        }

        if (testWaveCoroutine != null)
        {
            StopCoroutine(testWaveCoroutine);
        }
        testWaveCoroutine = StartCoroutine(RunTestWave(testWaveData));
        Debug.Log($"<color=cyan>[TestManager] 테스트 웨이브 시작: {testWaveData.name} (총 {testWaveData.GetTotalEnemyCount()}마리)</color>");
    }

    private IEnumerator RunTestWave(WaveDataSO waveData)
    {
        GridNode spawnNode = GridManager.Instance?.GetSpawnNode();
        if (spawnNode == null)
        {
            Debug.LogError("[TestManager] 스폰 노드를 찾을 수 없습니다.");
            yield break;
        }

        int totalSpawned = 0;
        foreach (var group in waveData.groups)
        {
            if (group.unitData == null) continue;

            if (group.initialDelay > 0)
                yield return new WaitForSeconds(group.initialDelay);

            for (int i = 0; i < group.count; i++)
            {
                UnitManager.Instance.SpawnUnit(group.unitData, spawnNode);
                totalSpawned++;
                Debug.Log($"<color=yellow>[TestWave] {group.unitData.unitName} 스폰 ({totalSpawned}/{waveData.GetTotalEnemyCount()})</color>");

                if (group.spawnInterval > 0 && i < group.count - 1)
                    yield return new WaitForSeconds(group.spawnInterval);
            }
        }

        Debug.Log("<color=cyan>[TestManager] 테스트 웨이브 스폰 완료.</color>");
    }
}
