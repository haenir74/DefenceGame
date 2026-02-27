using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Skill_GlobalBuff", menuName = "DungeonDefence/Skills/Global Buff")]
public class GlobalBuffSkillSO : SkillDataSO
{
    [UnityEngine.Serialization.FormerlySerializedAs("targetCategory")]
    public UnitTag targetTag = UnitTag.Spider;
    
    [UnityEngine.Serialization.FormerlySerializedAs("attackPowerMultiplier")]
    public float attackPowerBonusPercent = 0.2f;

    private Dictionary<Unit, HashSet<Unit>> appliedBuffs = new Dictionary<Unit, HashSet<Unit>>();

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitUpdate(Unit owner)
    {
        if (UnitManager.Instance == null) return;

        if (!appliedBuffs.ContainsKey(owner)) 
            appliedBuffs[owner] = new HashSet<Unit>();
            
        var ownerBuffs = appliedBuffs[owner];
        var allUnits = UnitManager.Instance.GetAllUnits();
        
        foreach (var unit in allUnits)
        {
            if (unit.IsPlayerTeam == owner.IsPlayerTeam && unit.Data != null && unit.Data.HasTag(targetTag))
            {
                if (!ownerBuffs.Contains(unit) && unit.Combat != null && unit.Combat.AttackPower != null)
                {
                    unit.Combat.AttackPower.AddModifier(new StatModifier(attackPowerBonusPercent, StatModType.PercentMultiply, owner));
                    ownerBuffs.Add(unit);
                }
            }
        }
    }

    public override void OnUnitDie(Unit owner)
    {
        if (appliedBuffs.ContainsKey(owner))
        {
            foreach (var unit in appliedBuffs[owner])
            {
                if (unit != null && unit.Combat != null && unit.Combat.AttackPower != null)
                {
                    unit.Combat.AttackPower.RemoveAllModifiersFromSource(owner);
                }
            }
            appliedBuffs.Remove(owner);
        }
    }
}



