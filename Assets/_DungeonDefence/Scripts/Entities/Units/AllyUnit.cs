using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyUnit : Unit
{
    // 기존 Update 로직은 부모(Unit.cs)의 상태머신으로 대체되므로 제거

    public override bool DetectTarget(out Unit target)
    {
        target = null;
        if (data == null) return false;

        // 최적화를 위해 Physics.OverlapSphere 또는 별도의 UnitManager 리스트를 사용하는 것이 좋지만,
        // 현재는 Scene의 모든 EnemyUnit을 검색 (프로토타입용)
        EnemyUnit[] enemies = FindObjectsOfType<EnemyUnit>();
        
        float minDst = float.MaxValue;
        float range = data.attackRange;

        foreach (var enemy in enemies)
        {
            if (enemy.IsDead) continue;

            float dst = Vector3.Distance(transform.position, enemy.transform.position);
            
            // 사거리 내에 있고, 가장 가까운 적 선택
            if (dst <= range && dst < minDst)
            {
                minDst = dst;
                target = enemy;
            }
        }

        return target != null;
    }
}