using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitCombat : MonoBehaviour
{
    private Unit unit;
    private UnitDataSO data;

    private float currentHp;
    private float attackTimer;

    public float CurrentHp => currentHp;
    public float MaxHp => data != null ? data.maxHp : 0f;

    public bool IsDead { get; private set; }

    public event Action OnDeath;
    public event Action<float> OnHpChanged;

    public void Initialize(Unit unit, UnitDataSO data)
    {
        this.unit = unit;
        this.data = data;
        
        this.currentHp = data.maxHp;
        this.attackTimer = 0f;
        IsDead = false;
    }

    public void OnUpdate()
    {
        if (IsDead) return;
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        currentHp -= amount;
        OnHpChanged?.Invoke(this.currentHp);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (this.IsDead) return;

        this.currentHp += amount;
        if (this.currentHp > this.data.maxHp) 
            this.currentHp = this.data.maxHp;

        OnHpChanged?.Invoke(this.currentHp);
    }

    public float GetHpRatio()
    {
        if (this.data == null || this.data.maxHp == 0) return 0f;
        return this.currentHp / this.data.maxHp;
    }

    public void Attack(Unit target)
    {
        if (attackTimer > 0 || target == null || target.IsDead) return;
        target.Combat.TakeDamage(data.basePower);
        attackTimer = data.attackInterval;
    }

    private void Die()
    {
        IsDead = true;
        OnDeath?.Invoke();
    }
}