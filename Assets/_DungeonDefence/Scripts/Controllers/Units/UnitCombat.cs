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
    public const string AttackRange = "AttackRange";
    public const string CritRate = "CritRate";
    public const string CritDamage = "CritDamage";
    public const string Armor = "Armor";
}

public class UnitCombat
{
    private Unit unit;
    private StatController statController;
    private float lastAttackTime;

    private Stat hpStat;
    private Stat damageStat;
    private Stat attackSpeedStat;
    private Stat attackRangeStat;
    private Stat critRateStat;
    private Stat critDamageStat;
    private Stat armorStat;

    public UnitCombat(Unit unit)
    {
        this.unit = unit;
        this.statController = unit.GetComponent<StatController>();
    }

    public void Initialize()
    {
        if (statController != null)
        {
            statController.InitializeStats();
        }
    }

    public void SyncStats(UnitDataSO data)
    {
        SetBaseValue("MaxHP", data.maxHp);
        SetBaseValue("AttackDamage", data.attackDamage);
        SetBaseValue("MoveSpeed", data.moveSpeed);
        SetBaseValue("AttackSpeed", data.attackSpeed);
        SetBaseValue("AttackRange", data.attackRange);
        SetBaseValue("CritDamage", data.critDamage);
        SetBaseValue("Armor", data.armor);
    }

    private void SetBaseValue(string key, float value)
    {
        if (statController == null) return;
        var stat = statController.GetStat(key);
        if (stat != null) stat.BaseValue = value;
    }

    private float GetValue(string key, float defaultValue)
    {
        return statController != null ? statController.GetValue(key) : defaultValue;
    }

    public float MaxHP => GetValue("MaxHP", 100f);
    public float Damage => GetValue("AttackDamage", 10f);
    public float MoveSpeed => GetValue("MoveSpeed", 2f);
    public float AttackRange => GetValue("AttackRange", 0.8f);
    public float AttackSpeed => GetValue("AttackSpeed", 1f);
    public float CritRate => GetValue("CritRate", 0f);
    public float CritDamage => GetValue("CritDamage", 50f);
    public float Armor => GetValue("Armor", 0f);

    public void TryAttack(Unit target)
    {
        if (target == null || target.IsDead) return;

        float speed = Mathf.Max(AttackSpeed, 0.1f);
        float cooldown = 1f / speed;

        if (Time.time >= lastAttackTime + cooldown)
        {
            PerformAttack(target);
            lastAttackTime = Time.time;
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

        unit.ReduceHealth(finalDmg);

        FloatingTextController.Instance?.Show(Mathf.RoundToInt(finalDmg).ToString(), unit.transform.position, isCrit);
    }

    public bool DetectTarget(out Unit target)
    {
        target = null;
        Node currentNode = unit.CurrentNode;
        if (currentNode == null) return false;

        Team targetTeam = (unit.MyTeam == Team.Ally) ? Team.Enemy : Team.Ally;

        foreach (var u in currentNode.UnitsOnNode)
        {
            if (u == unit || u.IsDead) continue;
            if (u.MyTeam == targetTeam)
            {
                target = u;
                return true;
            }
        }
        return false;
    }
}