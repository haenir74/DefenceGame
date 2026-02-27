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
    public float MaxHp
    {
        get
        {
            if (data == null) return 0f;
            float baseMax = data.maxHp;
            if (data.category == UnitCategory.Core && MetaManager.Instance != null)
            {
                baseMax += MetaManager.Instance.GetPerkLevel("CoreHP") * 100f;
            }
            return baseMax;
        }
    }
    public float CurrentMp => currentMp;
    public float MaxMp => data != null ? data.maxMp : 0f;

    public bool IsDead { get; private set; }

    public event Action OnDeath;
    public event Action<float> OnHpChanged;
    public event Action<Unit> OnAttack;
    public event Action<Unit, float> OnAttackHit;
    public event Action OnMaxMpReached;
    public float MpMultiplier { get; set; } = 1.0f;
    public float AttackMultiplier { get; set; } = 1.0f;

    public void Initialize(Unit unit, UnitDataSO data)
    {
        this.unit = unit;
        this.data = data;

        IsDead = false;
        this.currentHp = MaxHp;
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

    public void TakeDamage(float amount, Unit attacker = null)
    {
        if (IsDead || (unit != null && unit.IsDispatched)) return;

        currentHp -= amount;
        OnHpChanged?.Invoke(this.currentHp);

        if (currentHp <= 0)
        {
            Die();
            if (attacker != null)
            {
                attacker.Combat.NotifyKill(unit);
            }
        }
    }

    public void NotifyKill(Unit victim)
    {

        if (data != null && data.skill != null)
        {
            data.skill.OnUnitKill(unit, victim);
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

        float damageDealt = data.basePower * AttackMultiplier;
        target.Combat.TakeDamage(damageDealt, unit);

        OnAttackHit?.Invoke(target, damageDealt);

        OnAttack?.Invoke(target);
        attackTimer = data.attackInterval;

        var spriteAnimator = unit?.GetComponentInChildren<UnitSpriteAnimator>();
        if (spriteAnimator != null) spriteAnimator.TriggerAttackAnimation();

        var mecanimAnimator = unit?.GetComponentInChildren<UnitMecanimAnimator>();
        if (mecanimAnimator != null) mecanimAnimator.TriggerAttackAnimation();
    }

    public void AddMp(float amount)
    {
        if (IsDead || data == null || data.maxMp <= 0) return;
        currentMp = Mathf.Min(currentMp + amount, data.maxMp);

        if (currentMp >= data.maxMp)
        {
            OnMaxMpReached?.Invoke();
        }
    }

    public void ResetMp()
    {
        currentMp = 0f;
    }

    private void Die()
    {
        IsDead = true;
        OnDeath?.Invoke();
    }
}


