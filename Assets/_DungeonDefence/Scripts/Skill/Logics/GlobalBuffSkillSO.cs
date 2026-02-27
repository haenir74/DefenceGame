using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Skill_GlobalBuff", menuName = "DungeonDefence/Skills/Global Buff")]
public class GlobalBuffSkillSO : SkillDataSO
{
    public UnitTag targetTag = UnitTag.Spider;
    public float attackPowerBonusPercent = 0.2f;

    private HashSet<Unit> activeOwners = new HashSet<Unit>();

    private void OnEnable()
    {
        activeOwners.Clear();
    }

    private void OnDisable()
    {
        if (UnitManager.InstanceExists)
        {
            UnitManager.Instance.OnUnitSpawned -= HandleUnitSpawned;
            UnitManager.Instance.OnUnitDespawned -= HandleUnitDespawned;
        }
        activeOwners.Clear();
    }

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitUpdate(Unit owner)
    {
        if (!activeOwners.Contains(owner))
        {
            if (activeOwners.Count == 0 && UnitManager.Instance != null)
            {
                UnitManager.Instance.OnUnitSpawned += HandleUnitSpawned;
                UnitManager.Instance.OnUnitDespawned += HandleUnitDespawned;
            }
            activeOwners.Add(owner);

            if (UnitManager.Instance != null)
            {
                var existingUnits = UnitManager.Instance.GetUnitsByTag(targetTag);
                foreach (var unit in existingUnits)
                {
                    if (unit.IsPlayerTeam == owner.IsPlayerTeam && unit.Combat != null && unit.Combat.AttackPower != null)
                    {
                        unit.Combat.AttackPower.AddModifier(new StatModifier(attackPowerBonusPercent, StatModType.PercentMultiply, owner));
                    }
                }
            }
        }
    }

    public override void OnUnitDie(Unit owner)
    {
        if (activeOwners.Contains(owner))
        {
            activeOwners.Remove(owner);

            if (UnitManager.Instance != null)
            {
                var existingUnits = UnitManager.Instance.GetUnitsByTag(targetTag);
                foreach (var unit in existingUnits)
                {
                    if (unit != null && unit.Combat != null && unit.Combat.AttackPower != null)
                    {
                        unit.Combat.AttackPower.RemoveAllModifiersFromSource(owner);
                    }
                }

                if (activeOwners.Count == 0)
                {
                    UnitManager.Instance.OnUnitSpawned -= HandleUnitSpawned;
                    UnitManager.Instance.OnUnitDespawned -= HandleUnitDespawned;
                }
            }
        }
    }

    private void HandleUnitSpawned(Unit unit)
    {
        if (unit.Data != null && unit.Data.HasTag(targetTag))
        {
            foreach (var owner in activeOwners)
            {
                if (owner != null && !owner.IsDead && unit.IsPlayerTeam == owner.IsPlayerTeam)
                {
                    if (unit.Combat != null && unit.Combat.AttackPower != null)
                    {
                        unit.Combat.AttackPower.AddModifier(new StatModifier(attackPowerBonusPercent, StatModType.PercentMultiply, owner));
                    }
                }
            }
        }
    }

    private void HandleUnitDespawned(Unit unit)
    {
        if (unit.Data != null && unit.Data.HasTag(targetTag))
        {
            foreach (var owner in activeOwners)
            {
                if (owner != null && unit.Combat != null && unit.Combat.AttackPower != null)
                {
                    unit.Combat.AttackPower.RemoveAllModifiersFromSource(owner);
                }
            }
        }
    }
}



