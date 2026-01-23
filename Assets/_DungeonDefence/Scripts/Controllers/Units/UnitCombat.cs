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
    private Unit unit;
    private StatController statController;
    private float lastAttackTime;

    private Stat hpStat;
    private Stat damageStat;
    private Stat attackSpeedStat;
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
        if (statController == null) return;
        hpStat = statController.GetStat(StatKeys.MaxHP);
        damageStat = statController.GetStat(StatKeys.AttackDamage);
        attackSpeedStat = statController.GetStat(StatKeys.AttackSpeed);
        critRateStat = statController.GetStat(StatKeys.CritRate);
        critDamageStat = statController.GetStat(StatKeys.CritDamage);
        armorStat = statController.GetStat(StatKeys.Armor);
    }

    public void SyncStats(UnitDataSO data)
    {
        if (statController == null || data == null) return;
        if (hpStat != null) hpStat.BaseValue = data.maxHp;
        if (damageStat != null) damageStat.BaseValue = data.attackDamage;
        if (attackSpeedStat != null) attackSpeedStat.BaseValue = data.attackSpeed;
        if (critRateStat != null) critRateStat.BaseValue = data.critRate;
        if (critDamageStat != null) critDamageStat.BaseValue = data.critDamage;
        if (armorStat != null) armorStat.BaseValue = data.armor;
        var moveSpeedStat = statController.GetStat(StatKeys.MoveSpeed);
        if (moveSpeedStat != null) moveSpeedStat.BaseValue = data.moveSpeed;
    }

    public float MaxHP => hpStat?.Value ?? 100f;
    public float Damage => damageStat?.Value ?? 10f;
    public float AttackSpeed => attackSpeedStat?.Value ?? 1f;
    public float CritRate => critRateStat?.Value ?? 0f;
    public float CritDamage => critDamageStat?.Value ?? 50f;
    public float Armor => armorStat?.Value ?? 0f;
    public float MoveSpeed => statController.GetValue(StatKeys.MoveSpeed);

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