using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DefenderUnit : Unit
{
    [Header("Combat Stats")]
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;

    private float lastAttackTime;

    protected override void Update()
    {
        base.Update();
        
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        EnemyUnit target = FindNearestEnemy();
        if (target != null)
        {
            Attack(target);
            lastAttackTime = Time.time;
        }
    }

    private EnemyUnit FindNearestEnemy()
    {
        // 최적화를 위해 Physics.OverlapSphere 또는 별도의 UnitManager 리스트를 사용하는 것이 좋지만,
        // 현재는 Scene의 모든 EnemyUnit을 검색 (프로토타입용)
        EnemyUnit[] enemies = FindObjectsOfType<EnemyUnit>();
        
        EnemyUnit nearest = null;
        float minDst = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (enemy.IsDead) continue;

            float dst = Vector3.Distance(transform.position, enemy.transform.position);
            if (dst <= attackRange && dst < minDst)
            {
                minDst = dst;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void Attack(Unit target)
    {
        // 시각적 효과 (추후 구현)
        Debug.Log($"{name} attacks {target.name}!");
        
        target.TakeDamage(attackDamage);
    }
}
