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

    private float runtimeMaxHp;

    public float CurrentHp => currentHp;
    public float MaxHp => runtimeMaxHp;
    public float CurrentMp => currentMp;
    public float MaxMp => data != null ? data.maxMp : 0f;

    public bool IsDead { get; private set; }

    public event Action OnDeath;
    public event Action<float> OnHpChanged;
    public event Action<Unit> OnAttack;
    public event Action<Unit, float> OnAttackHit;
    public event Action OnMaxMpReached;
    public float MpMultiplier { get; set; } = 1.0f;
    public CharacterStat AttackPower { get; private set; }

    public void Initialize(Unit unit, UnitDataSO data)
    {
        this.unit = unit;
        this.data = data;

        IsDead = false;

        runtimeMaxHp = data != null ? data.maxHp : 0f;
        float basePower = data != null ? data.basePower : 0f;

        if (unit != null && unit.IsPlayerTeam && MetaManager.Instance != null && data != null)
        {
            if (data.category == UnitCategory.Core)
            {
                runtimeMaxHp += MetaManager.Instance.GetTotalBonusCoreHealth();
            }
            else
            {
                runtimeMaxHp += MetaManager.Instance.GetTotalBonusAllyHealth();
                basePower += MetaManager.Instance.GetTotalBonusAllyAttack();
            }
        }

        this.currentHp = runtimeMaxHp;
        this.currentMp = data != null ? data.startMp : 0f;
        this.attackTimer = 0f;
        this.MpMultiplier = 1.0f;

        this.AttackPower = new CharacterStat(basePower);
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

        float damageDealt = AttackPower != null ? AttackPower.Value : (data != null ? data.basePower : 0f);

        // Spider Synergy: 거미줄 위에서 공격 시 피해량 증가
        if (data != null && data.race == UnitRace.Spider && unit.CurrentNode != null)
        {
            var tileData = GridManager.Instance.GetTileData(unit.CurrentNode.Coordinate);
            if (tileData != null && tileData.environment == TileEnvironment.SpiderWeb)
            {
                damageDealt *= 1.5f; // 50% 추가 피해 보너스
            }
        }

        target.Combat.TakeDamage(damageDealt, unit);

        OnAttackHit?.Invoke(target, damageDealt);

        // Attack 적중 시 기본 마나 획득
        AddMp(10f * MpMultiplier);

        OnAttack?.Invoke(target);

        // AttackSpeed 기반 쿨다운 계산
        float aspd = data != null && data.attackSpeed > 0f ? data.attackSpeed : 1f;
        attackTimer = (data != null ? data.attackInterval : 1f) / aspd;

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
            ResetMp(); // 스킬 시전 알림 후 MP 초기화
        }
    }

    public void ResetMp()
    {
        currentMp = 0f;
    }

    private void Die()
    {
        IsDead = true;

        // Slime Synergy: 늪지 위에서 사망 시 골드 환급
        if (data != null && data.race == UnitRace.Slime && unit != null && unit.CurrentNode != null)
        {
            var tileData = GridManager.Instance.GetTileData(unit.CurrentNode.Coordinate);
            if (tileData != null && tileData.environment == TileEnvironment.Swamp)
            {
                // 소모 골드의 일부를 환급 (예: 판매가의 절반 정도를 환급한다고 가정, 여기서는 임시로 10 골드로 설정)
                int refundAmount = 10;
                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddCurrency(CurrencyType.Gold, refundAmount);
                }
            }
        }

        OnDeath?.Invoke();
    }
}


