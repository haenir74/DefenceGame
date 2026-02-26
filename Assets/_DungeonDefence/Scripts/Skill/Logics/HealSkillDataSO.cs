using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Heal Skill", menuName = "DungeonDefence/Skill/Heal")]
public class HealSkillDataSO : SkillDataSO
{
    
    public float healAmount = 30f;

    public override void Cast(Unit caster, Unit mainTarget)
    {
        var allies = GetAllies(caster);

        var targetAlly = allies
            .OrderBy(u => UnitManager.Instance.GetUnitHpRatio(u))
            .FirstOrDefault();

        if (targetAlly != null)
        {
            
            UnitManager.Instance.HealUnit(targetAlly, healAmount);
        }
        else
        {
            
        }
    }
}


