using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class UnitCombat : MonoBehaviour
{
    private Unit _unit;
    private UnitDataSO _data;

    public float CurrentHp { get; private set; }
    public float MaxHp { get; private set; }
    public bool IsDead => CurrentHp <= 0;

    private float _lastAttackTime;

    public event Action<float> OnHealthChanged; // float: 비율(0~1)
    public event Action OnDeath;
    public event Action OnAttack;

    public void Setup(Unit unit, UnitDataSO data)
    {
        _unit = unit;
        _data = data;

        MaxHp = data.maxHp;
        CurrentHp = MaxHp;
        _lastAttackTime = -999f;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHp -= amount;
        CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp);

        OnHealthChanged?.Invoke(CurrentHp / MaxHp);

        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    public bool TryAttack(Unit target)
    {
        if (IsDead || target == null || target.Combat.IsDead) return false;

        if (Time.time < _lastAttackTime + _data.attackInterval) return false;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > _data.attackRange) return false;

        PerformAttack(target);
        return true;
    }

    private void PerformAttack(Unit target)
    {
        _lastAttackTime = Time.time;
        OnAttack?.Invoke();

        target.Combat.TakeDamage(_data.attackDamage);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        
        Destroy(gameObject); // 일단 단순 파괴, 오브젝트 풀링 만든 후 변경
    }
}