using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitCombat : MonoBehaviour
{
    private Unit unit;
    private UnitDataSO data;

    private float currentHp;
    private float currentMp;
    private float attackTimer;

    public float CurrentHp => currentHp;
    public float MaxHp => data != null ? data.maxHp : 0f;
    public float CurrentMp => currentMp;
    public float MaxMp => data != null ? data.maxMp : 0f;

    public bool IsDead { get; private set; }

    public event Action OnDeath;
    public event Action<float> OnHpChanged;

    public void Initialize(Unit unit, UnitDataSO data)
    {
        this.unit = unit;
        this.data = data;

        IsDead = false;
        this.currentHp = data != null ? data.maxHp : 100f;
        this.currentMp = data != null ? data.startMp : 0f;
        this.attackTimer = 0f;
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
        if (IsDead || (unit != null && unit.IsDispatched)) return;

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
        if (unit != null && unit.IsDispatched) return;
        if (attackTimer > 0 || target == null || target.IsDead) return;

        // 기본 공격
        target.Combat.TakeDamage(data.basePower);
        attackTimer = data.attackInterval;

        // MP 충전 (스킬이 있을 때만)
        if (data.skill != null && data.maxMp > 0)
        {
            currentMp += data.maxMp * 0.25f; // 기본 공격 4회에 스킬 1회
            if (currentMp >= data.maxMp)
            {
                currentMp = 0f;
                TryCastSkill(target);
            }
        }
    }

    private void TryCastSkill(Unit mainTarget)
    {
        if (data.skill == null) return;
        data.skill.Cast(unit, mainTarget);
    }

    private void Die()
    {
        IsDead = true;
        OnDeath?.Invoke();
    }
}