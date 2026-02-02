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

    public float CurrentMana { get; private set; }
    public float MaxMana { get; private set; }

    private float _lastAttackTime;

    private const float MANA_PER_ATTACK = 10f;

    public event Action<float> OnHealthChanged; // float: 비율(0~1)
    public event Action<float> OnManaChanged;
    public event Action OnDeath;
    public event Action OnAttack;
    public event Action OnSkillCast;

    public void Setup(Unit unit, UnitDataSO data)
    {
        _unit = unit;
        _data = data;

        MaxHp = data.maxHp;
        CurrentHp = MaxHp;
        MaxMana = data.maxMp;
        CurrentMana = data.startMp;

        _lastAttackTime = -999f;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHp -= amount;
        CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp);

        OnHealthChanged?.Invoke(CurrentHp / MaxHp);

        AddMana(5f);

        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    public void AddMana(float amount)
    {
        if (MaxMana <= 0) return;

        CurrentMana += amount;
        CurrentMana = Mathf.Clamp(CurrentMana, 0, MaxMana);
        
        OnManaChanged?.Invoke(CurrentMana / MaxMana);
    }

    public bool TryAttack(Unit target)
    {
        if (IsDead || target == null || target.Combat.IsDead) return false;

        if (Time.time < _lastAttackTime + _data.attackInterval) return false;

        if (CurrentMana >= MaxMana && _data.skill != null)
        {
            PerformSkill(target);
        }
        else
        {
            PerformAttack(target);
        }

        return true;
    }

    private void PerformAttack(Unit target)
    {
        _lastAttackTime = Time.time;
        OnAttack?.Invoke();

        target.Combat.TakeDamage(_data.basePower);
        AddMana(MANA_PER_ATTACK);
    }

    private void PerformSkill(Unit target)
    {
        _lastAttackTime = Time.time; 
        
        CurrentMana = 0f;
        OnManaChanged?.Invoke(0f);
        
        OnSkillCast?.Invoke();

        if (_data.skill != null)
        {
            _data.skill.Cast(_unit, target);
        }
        
        Debug.Log($"{_unit.name} used Skill: {_data.skill.skillName}!");
    }

    private void Die()
    {
        OnDeath?.Invoke();
        
        Destroy(gameObject); // 일단 단순 파괴, 오브젝트 풀링 만든 후 변경
    }
}