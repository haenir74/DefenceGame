using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyUnit : Unit
{
    private float lastAttackTime;

    protected override void Update()
    {
        base.Update();
        
        if (data == null) return;

        if (Time.time >= lastAttackTime + data.attackCooldown)
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
        EnemyUnit[] enemies = FindObjectsOfType<EnemyUnit>();
        
        EnemyUnit nearest = null;
        float minDst = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (enemy.IsDead) continue;

            float dst = Vector3.Distance(transform.position, enemy.transform.position);
            if (dst <= data.attackRange && dst < minDst)
            {
                minDst = dst;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void Attack(Unit target)
    {
        Debug.Log($"{name} attacks {target.name}!");
        target.TakeDamage(data.attackDamage);
    }
}
