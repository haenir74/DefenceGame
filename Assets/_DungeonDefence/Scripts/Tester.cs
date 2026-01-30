using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntegrationTester : MonoBehaviour
{
    [Header("Test Data")]
    [SerializeField] private UnitDataSO playerUnitData;
    [SerializeField] private UnitDataSO enemyUnitData;

    // 현재 선택된 유닛 (마지막으로 소환하거나 클릭한 유닛)
    private Unit _selectedUnit;

    private void Start()
    {
        StartCoroutine(SetupTest());
    }

    private IEnumerator SetupTest()
    {
        // 시스템 초기화 대기 (안전장치)
        yield return new WaitForSeconds(0.5f);

        if (InputManager.Instance != null)
        {
            // InputManager 이벤트 구독
            InputManager.Instance.OnClickNode += HandleLeftClick;
            InputManager.Instance.OnRightClickNode += HandleRightClick;
            
            Debug.Log("=== [Integration Test Controls] ===");
            Debug.Log("[L-Click]: 빈 땅이면 아군 소환 / 유닛 있으면 선택");
            Debug.Log("[R-Click]: 선택된 유닛이 있다면 그곳으로 '이동' / 없다면 적군 소환");
            Debug.Log("[Space]: 모든 유닛 랜덤 이동");
            Debug.Log("[K Key]: 모든 유닛 데미지 (Kill Test)");
            Debug.Log("[A Key]: 공격 시도 로그 (Attack Test)");
        }
        else
        {
            Debug.LogError("[IntegrationTester] InputManager를 찾을 수 없습니다.");
        }
    }

    // 좌클릭 핸들러: 유닛 선택 또는 아군 소환
    private void HandleLeftClick(GridNode node)
    {
        if (node == null) return;

        // 1. 해당 타일에 유닛이 있는지 확인
        Unit foundUnit = FindUnitOnNode(node);

        if (foundUnit != null)
        {
            _selectedUnit = foundUnit;
            Debug.Log($"[Tester] 유닛 선택됨: {_selectedUnit.name} ({node.X}, {node.Y})");
        }
        else
        {
            // 빈 땅이면 아군 소환
            if (playerUnitData != null)
            {
                _selectedUnit = UnitManager.Instance.SpawnUnit(playerUnitData, node);
                Debug.Log($"[Tester] 아군 소환 및 선택: {_selectedUnit.name} at ({node.X}, {node.Y})");
            }
        }
    }

    // 우클릭 핸들러: 유닛 이동 또는 적군 소환
    private void HandleRightClick(GridNode node)
    {
        if (node == null) return;

        // 선택된 유닛이 있고 살아있다면 -> 이동 명령
        if (_selectedUnit != null && _selectedUnit.Combat != null && !_selectedUnit.Combat.IsDead)
        {
            Debug.Log($"[Tester] {_selectedUnit.name}에게 ({node.X}, {node.Y})로 이동 명령");
            
            // FSM을 사용하는 경우:
            // _selectedUnit.FSM.ChangeState(new UnitMoveState(node)); 
            
            // Movement 컴포넌트 직접 사용:
            _selectedUnit.Movement.MoveTo(node, () => Debug.Log($"{_selectedUnit.name} 이동 완료!")); 
        }
        else
        {
            // 선택된 유닛이 없으면 -> 적군 소환
            if (enemyUnitData != null)
            {
                UnitManager.Instance.SpawnUnit(enemyUnitData, node);
                Debug.Log($"[Tester] 적군 소환: Enemy at ({node.X}, {node.Y})");
            }
        }
    }

    private void Update()
    {
        // [Space] 모든 유닛 랜덤 이동
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MoveAllUnitsRandomly();
        }

        // [K] Kill Test (데미지 10)
        if (Input.GetKeyDown(KeyCode.K))
        {
            DamageAllUnits();
        }
        
        // [A] Attack Test (로그 출력)
        if (Input.GetKeyDown(KeyCode.A))
        {
            ForceAttackTest();
        }
    }

    // --- Helper Methods ---

    // 노드 위에 있는 유닛 찾기 (임시 검색 로직)
    private Unit FindUnitOnNode(GridNode node)
    {
        var allUnits = UnitManager.Instance.GetAllUnits();
        foreach (var unit in allUnits)
        {
            // 1. 논리적 노드 비교
            if (unit.CurrentNode == node) return unit;
            
            // 2. (보정) 물리적 거리 비교 (0.5f 이내)
            if (Vector3.Distance(unit.transform.position, node.WorldPosition) < 0.5f) return unit;
        }
        return null;
    }

    private void MoveAllUnitsRandomly()
    {
        Debug.Log("[Tester] 모든 유닛 랜덤 이동 시작");
        var units = UnitManager.Instance.GetAllUnits();
        foreach (var unit in units)
        {
            GridNode current = unit.CurrentNode;
            if (current == null) continue;
            
            // 랜덤 인접 타일 계산
            int rx = current.X + Random.Range(-1, 2);
            int ry = current.Y + Random.Range(-1, 2);
            
            // GridManager에 오버로딩된 GetNode(int, int) 사용
            GridNode target = GridManager.Instance.GetNode(rx, ry);
            
            if (target != null && target != current)
            {
                unit.Movement.MoveTo(target);
            }
        }
    }

    private void DamageAllUnits()
    {
        Debug.Log("[Tester] 모든 유닛에게 데미지 10 적용");
        // 리스트를 복사해서 순회 (사망 시 리스트 변경 가능성 대비)
        var units = new List<Unit>(UnitManager.Instance.GetAllUnits());
        
        foreach (var unit in units)
        {
            if (unit.Combat != null)
            {
                unit.Combat.TakeDamage(10);
                Debug.Log($"Unit {unit.name} HP: {unit.Combat.CurrentHp}/{unit.Combat.MaxHp}");
            }
        }
    }

    private void ForceAttackTest()
    {
        Debug.Log("[Tester] 공격 상태 확인");
        var units = UnitManager.Instance.GetAllUnits();
        foreach (var unit in units)
        {
            if (unit.Combat != null)
            {
                Debug.Log($"{unit.name} - Attack Interval: {unit.Data.attackInterval}, Damage: {unit.Data.attackDamage}");
            }
        }
    }
}