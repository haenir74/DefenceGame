using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntegrationTester : MonoBehaviour
{
    [Header("Test Data")]
    [SerializeField] private UnitDataSO playerUnitData;
    [SerializeField] private UnitDataSO enemyUnitData;

    private void Start()
    {
        StartCoroutine(SetupTest());
    }

    private IEnumerator SetupTest()
    {
        // 시스템 초기화 대기
        yield return new WaitForSeconds(0.5f);

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += HandleLeftClick;     // 아군 소환
            InputManager.Instance.OnRightClickNode += HandleRightClick; // 적군 소환
            Debug.Log("[Tester] 시스템 준비 완료. (좌클릭: 아군 / 우클릭: 적군 / Space: 이동 / K: 데미지 / A: 공격시도)");
        }
    }

    private void HandleLeftClick(GridNode node)
    {
        if (node != null && playerUnitData != null)
            UnitManager.Instance.SpawnUnit(playerUnitData, node);
    }

    private void HandleRightClick(GridNode node)
    {
        if (node != null && enemyUnitData != null)
            UnitManager.Instance.SpawnUnit(enemyUnitData, node);
    }

    private void Update()
    {
        // [Space] 랜덤 이동
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MoveAllUnitsRandomly();
        }

        // [K] Kill Test (데미지)
        if (Input.GetKeyDown(KeyCode.K))
        {
            DamageAllUnits();
        }
        
        // [A] Attack Test
        if (Input.GetKeyDown(KeyCode.A))
        {
            ForceAttackTest();
        }
    }

    private void MoveAllUnitsRandomly()
    {
        Debug.Log("[Tester] 유닛 이동 명령!");
        var units = UnitManager.Instance.GetAllUnits();
        foreach (var unit in units)
        {
            GridNode current = unit.CurrentNode;
            if (current == null) continue;
            
            // 랜덤 인접 타일로 이동
            int rx = current.X + Random.Range(-1, 2);
            int ry = current.Y + Random.Range(-1, 2);
            GridNode target = GridManager.Instance.GetNode(rx, ry);
            
            if (target != null && target != current)
            {
                unit.Movement.MoveTo(target);
            }
        }
    }

    private void DamageAllUnits()
    {
        Debug.Log("[Tester] 모든 유닛에게 데미지 10!");
        var units = UnitManager.Instance.GetAllUnits();
        // 리스트를 복사해서 순회 (죽어서 리스트에서 빠질 수 있으므로)
        var unitsCopy = new List<Unit>(units); 
        
        foreach (var unit in unitsCopy)
        {
            if (unit.Combat != null)
            {
                unit.Combat.TakeDamage(10);
                Debug.Log($"Unit {unit.name} HP: {unit.Combat.CurrentHp}");
            }
        }
    }

    private void ForceAttackTest()
    {
        Debug.Log("[Tester] 공격 시도!");
        var units = UnitManager.Instance.GetAllUnits();
        foreach (var unit in units)
        {
            // 실제로는 FSM에서 처리하겠지만, 강제로 가장 가까운 적을 찾아 공격해봄
            // 여기선 간단히 로그만
            if (unit.Combat != null)
            {
                // unit.Combat.TryAttack(target); // 타겟이 필요함
                Debug.Log($"{unit.name} 공격 준비 완료 (AttackInterval: {unit.Data.attackInterval})");
            }
        }
    }
}