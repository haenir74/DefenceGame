using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Panex.Stat.Model;
using Panex.Stat.Controller;
using Panex.FloatingText;

public static class StatKeys
{
    public const string MaxHP = "MaxHP";
    public const string MaxMP = "MaxMP";
    public const string AttackDamage = "AttackDamage";
    public const string MoveSpeed = "MoveSpeed";
    public const string AttackSpeed = "AttackSpeed";
    public const string CritRate = "CritRate";
    public const string CritDamage = "CritDamage";
    public const string Armor = "Armor";
}

public class UnitCombat
{
    private Unit _unit;
    private StatController _statController;
    private float _lastAttackTime;

    private Stat _hpStat;
    private Stat _damageStat;
    private Stat _attackSpeedStat;
    private Stat _critRateStat;
    private Stat _critDamageStat;
    private Stat _armorStat;

    public UnitCombat(Unit unit)
    {
        _unit = unit;
        _statController = unit.GetComponent<StatController>();
    }

    public void Initialize()
    {
        if (_statController == null) return;
        _hpStat = _statController.GetStat(StatKeys.MaxHP);
        _damageStat = _statController.GetStat(StatKeys.AttackDamage);
        _attackSpeedStat = _statController.GetStat(StatKeys.AttackSpeed);
        _critRateStat = _statController.GetStat(StatKeys.CritRate);
        _critDamageStat = _statController.GetStat(StatKeys.CritDamage);
        _armorStat = _statController.GetStat(StatKeys.Armor);
    }

    public void SyncStats(UnitDataSO data)
    {
        if (_statController == null || data == null) return;
        if (_hpStat != null) _hpStat.BaseValue = data.maxHp;
        if (_damageStat != null) _damageStat.BaseValue = data.attackDamage;
        if (_attackSpeedStat != null) _attackSpeedStat.BaseValue = data.attackSpeed;
        if (_critRateStat != null) _critRateStat.BaseValue = data.critRate;
        if (_critDamageStat != null) _critDamageStat.BaseValue = data.critDamage;
        if (_armorStat != null) _armorStat.BaseValue = data.armor;
        var moveSpeedStat = _statController.GetStat(StatKeys.MoveSpeed);
        if (moveSpeedStat != null) moveSpeedStat.BaseValue = data.moveSpeed;
    }

    public float MaxHP => _hpStat?.Value ?? 100f;
    public float Damage => _damageStat?.Value ?? 10f;
    public float AttackSpeed => _attackSpeedStat?.Value ?? 1f;
    public float CritRate => _critRateStat?.Value ?? 0f;
    public float CritDamage => _critDamageStat?.Value ?? 50f;
    public float Armor => _armorStat?.Value ?? 0f;
    public float MoveSpeed => _statController.GetValue(StatKeys.MoveSpeed);

    public void TryAttack(Unit target)
    {
        if (target == null || target.IsDead) return;

        float speed = Mathf.Max(AttackSpeed, 0.1f);
        float cooldown = 1f / speed;

        if (Time.time >= _lastAttackTime + cooldown)
        {
            PerformAttack(target);
            _lastAttackTime = Time.time;
        }
    }

    private void PerformAttack(Unit target)
    {
        float dmg = Damage;
        bool isCrit = UnityEngine.Random.Range(0f, 100f) < CritRate;

        if (isCrit)
        {
            dmg *= (1f + CritDamage / 100f);
        }

        target.TakeDamage(dmg, isCrit);
    }

    public void TakeDamage(float rawDmg, bool isCrit)
    {
        float currentArmor = Armor;
        float mitigation = 100f / (100f + Mathf.Max(0, currentArmor));
        float finalDmg = rawDmg * mitigation;

        _unit.ReduceHealth(finalDmg);

        FloatingTextController.Instance?.Show(Mathf.RoundToInt(finalDmg).ToString(), _unit.transform.position, isCrit);
    }

    public bool DetectTarget(out Unit target)
    {
        target = null;
        Node currentNode = _unit.CurrentNode;
        if (currentNode == null) return false;

        Team targetTeam = (_unit.MyTeam == Team.Ally) ? Team.Enemy : Team.Ally;

        foreach (var u in currentNode.UnitsOnNode)
        {
            if (u == _unit || u.IsDead) continue;
            if (u.MyTeam == targetTeam)
            {
                target = u;
                return true;
            }
        }
        return false;
    }
}
