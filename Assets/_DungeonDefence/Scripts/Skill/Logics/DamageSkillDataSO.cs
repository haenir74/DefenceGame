using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Damage Skill", menuName = "DungeonDefence/Skill/Damage")]
public class DamageSkillDataSO : SkillDataSO
{
    
    public float damageAmount = 50f;

    public override void Cast(Unit caster, Unit mainTarget)
    {
        if (mainTarget != null)
        {
            
            UnitManager.Instance.DamageUnit(mainTarget, damageAmount);
        }
    }
}


