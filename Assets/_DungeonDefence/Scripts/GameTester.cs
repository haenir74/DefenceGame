using UnityEngine;
using System.Collections.Generic;

public class GameLoopTester : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float killRadius = 100f;
    [SerializeField] private LayerMask enemyLayer; // 적 유닛 레이어 설정 필요

    private void Update()
    {
        // 1. 자원 추가 테스트
        if (Input.GetKeyDown(KeyCode.F1))
        {
            EconomyManager.Instance?.AddCurrency(CurrencyType.Gold, 1000);
            Debug.Log("[Test] 골드 1000 추가");
        }

        // 2. 웨이브 강제 클리어 (모든 적 처치)
        if (Input.GetKeyDown(KeyCode.F2))
        {
            KillAllEnemies();
        }

        // 3. 게임 속도 조절
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Time.timeScale = (Time.timeScale > 1.0f) ? 1.0f : 2.0f;
            Debug.Log($"[Test] 시간 배속: {Time.timeScale}");
        }
        
        // 4. 강제 패배 테스트 (코어 데미지)
        if (Input.GetKeyDown(KeyCode.F4))
        {
            // UnitManager나 GridManager를 통해 코어를 찾아 데미지를 주는 로직
            // 현재 구조상 UnitManager에 코어 피격 로직이 있다면 호출
            Debug.Log("[Test] 코어 데미지 테스트 (구현 필요)");
        }
    }

    private void KillAllEnemies()
    {
        // 화면 내의 모든 적을 찾아서 즉사시킴
        // (Unit 컴포넌트의 TakeDamage 등을 호출)
        
        // 방법 A: UnitManager가 관리하는 리스트가 있다면 그것을 순회
        // 방법 B: Physics로 검색 (간편함)
        
        Collider[] colliders = Physics.OverlapSphere(Vector3.zero, killRadius, enemyLayer);
        int killCount = 0;

        foreach (var col in colliders)
        {
            Unit unit = col.GetComponent<Unit>();
            if (unit != null && !unit.IsPlayerTeam) // 적군만
            {
                // 체력보다 큰 데미지를 줘서 사망 로직(OnUnitDead -> WaveManager)이 자연스럽게 돌게 함
                // unit.TakeDamage(99999); 
                // 혹은 Unit 스크립트에 Die()가 있다면 호출
                
                // 임시: Unit 스크립트 내부를 볼 수 없으므로 Destroy 처리 혹은 로직 호출
                // 정상적인 흐름 테스트를 위해선 TakeDamage가 가장 좋습니다.
                var combat = unit.GetComponent<UnitCombat>(); // UnitCombat이 있다면
                if (combat != null) 
                {
                    // combat.TakeDamage(99999); // 접근 가능하다면
                }
                else
                {
                    // 차선책: 강제 비활성화 및 매니저 알림 (위험할 수 있음)
                    Destroy(unit.gameObject); 
                }
                killCount++;
            }
        }
        Debug.Log($"[Test] 적 {killCount}마리 처치 (웨이브 클리어 유도)");
    }
}