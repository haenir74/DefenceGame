using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private UnitDataSO enemyData; // 테스트용 적 데이터 할당
    [SerializeField] private UnitDataSO towerData; // 테스트용 타워 데이터 할당
    
    [Header("Spawn Settings")]
    [SerializeField] private int towerSpawnX = 5;
    [SerializeField] private int towerSpawnY = 5;

    private void Update()
    {
        // 1. 적 소환 테스트 (Spawn Node에서 소환)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SpawnEnemyTest();
        }

        // 2. 타워 소환 테스트 (지정된 좌표에 소환)
        if (Input.GetKeyDown(KeyCode.W))
        {
            SpawnTowerTest();
        }

        // 3. 웨이브 시작 테스트
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartWaveTest();
        }

        // 4. 골드 획득 테스트
        if (Input.GetKeyDown(KeyCode.R))
        {
            AddGoldTest();
        }

        // 5. 광역 데미지 테스트 (전투 및 사망 로직 확인)
        if (Input.GetKeyDown(KeyCode.T))
        {
            DamageAllEnemiesTest();
        }
        
        // 6. 게임 속도 토글
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleSpeed();
        }
    }

    private void SpawnEnemyTest()
    {
        if (GridManager.Instance == null || UnitManager.Instance == null) return;
        if (enemyData == null)
        {
            Debug.LogWarning("[Tester] EnemyData가 할당되지 않았습니다!");
            return;
        }

        GridNode spawnNode = GridManager.Instance.GetSpawnNode();
        if (spawnNode != null)
        {
            Unit enemy = UnitManager.Instance.SpawnUnit(enemyData, spawnNode);
            Debug.Log($"[Tester] 적 소환: {enemy.name} at {spawnNode.Coordinate}");
        }
        else
        {
            Debug.LogError("[Tester] Spawn Node를 찾을 수 없습니다.");
        }
    }

    private void SpawnTowerTest()
    {
        if (GridManager.Instance == null || UnitManager.Instance == null) return;
        if (towerData == null)
        {
            Debug.LogWarning("[Tester] TowerData가 할당되지 않았습니다!");
            return;
        }

        // 테스트를 위해 강제로 좌표 지정 (유효성 체크 포함)
        if (GridManager.Instance.IsValidNode(towerSpawnX, towerSpawnY))
        {
            Unit tower = UnitManager.Instance.SpawnUnit(towerData, towerSpawnX, towerSpawnY);
            Debug.Log($"[Tester] 타워 소환: {tower.name} at ({towerSpawnX}, {towerSpawnY})");
        }
        else
        {
            Debug.LogWarning($"[Tester] ({towerSpawnX}, {towerSpawnY})는 유효하지 않거나 이미 점유된 노드입니다.");
        }
    }

    private void StartWaveTest()
    {
        if (WaveManager.Instance == null) return;
        
        Debug.Log("[Tester] 웨이브 강제 시작 (Wave 1, 5 enemies)");
        WaveManager.Instance.StartWave(1, 5);
    }

    private void AddGoldTest()
    {
        if (GameManager.Instance == null) return;

        Debug.Log("[Tester] 골드 1000 추가");
        GameManager.Instance.AddGold(1000);
    }

    private void DamageAllEnemiesTest()
    {
        if (UnitManager.Instance == null) return;

        Debug.Log("[Tester] 모든 적에게 50 데미지 적용");
        var units = UnitManager.Instance.GetAllUnits();
        
        // 리스트를 순회하며 적군만 타격 (복사본을 쓰거나 역순으로 돌 필요가 있을 수 있음 - 리스트 변경 안전을 위해)
        for (int i = units.Count - 1; i >= 0; i--)
        {
            Unit unit = units[i];
            if (unit != null && !unit.IsPlayerTeam && !unit.IsDead)
            {
                UnitManager.Instance.DamageUnit(unit, 50f);
            }
        }
    }
    
    private void ToggleSpeed()
    {
        float current = Time.timeScale;
        Time.timeScale = current > 1.0f ? 1.0f : 2.0f;
        Debug.Log($"[Tester] 시간 가속: {Time.timeScale}x");
    }
}