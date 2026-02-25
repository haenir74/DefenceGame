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
    public float MpMultiplier { get; set; } = 1.0f;
    public float AttackMultiplier { get; set; } = 1.0f;

    public void Initialize(Unit unit, UnitDataSO data)
    {
        this.unit = unit;
        this.data = data;

        IsDead = false;
        this.currentHp = data != null ? data.maxHp : 100f;
        this.currentMp = data != null ? data.startMp : 0f;
        this.attackTimer = 0f;
        this.MpMultiplier = 1.0f;
        this.AttackMultiplier = 1.0f;
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

        bool hasSkill = data.skill != null && data.maxMp > 0;

        if (hasSkill && currentMp >= data.maxMp)
        {
            // 마나 가득 → 스킬 발동 (기본 공격 대체)
            currentMp = 0f;
            data.skill.Cast(unit, target);
        }
        else
        {
            // 기본 공격
            target.Combat.TakeDamage(data.basePower * AttackMultiplier);

            // 기본 공격 1회당 10MP 충전
            if (hasSkill)
            {
                AddMp(10f * MpMultiplier);
            }
        }

        attackTimer = data.attackInterval;

        // 공격 애니메이션 트리거 (슬라임 squash 또는 Mecanim 트리거)
        var spriteAnimator = unit?.GetComponentInChildren<UnitSpriteAnimator>();
        if (spriteAnimator != null) spriteAnimator.TriggerAttackAnimation();

        var mecanimAnimator = unit?.GetComponentInChildren<UnitMecanimAnimator>();
        if (mecanimAnimator != null) mecanimAnimator.TriggerAttackAnimation();
    }

    public void AddMp(float amount)
    {
        if (IsDead || data == null || data.maxMp <= 0) return;
        currentMp = Mathf.Min(currentMp + amount, data.maxMp);
    }

    private void Die()
    {
        IsDead = true;
        OnDeath?.Invoke();
    }
}