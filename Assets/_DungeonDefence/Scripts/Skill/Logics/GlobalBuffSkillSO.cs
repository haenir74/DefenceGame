using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Skill_GlobalBuff", menuName = "DungeonDefence/Skills/Global Buff")]
public class GlobalBuffSkillSO : SkillDataSO
{
    
    public UnitCategory targetCategory = UnitCategory.Spider;
    public float attackPowerMultiplier = 1.2f;

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitUpdate(Unit owner)
    {
        
        
        if (UnitManager.Instance == null) return;

        var allUnits = UnitManager.Instance.GetAllUnits();
        foreach (var unit in allUnits)
        {
            if (unit.IsPlayerTeam == owner.IsPlayerTeam && unit.Data != null && unit.Data.category == targetCategory)
            {
                
                unit.Combat.AttackMultiplier = attackPowerMultiplier;
            }
        }
    }
}



