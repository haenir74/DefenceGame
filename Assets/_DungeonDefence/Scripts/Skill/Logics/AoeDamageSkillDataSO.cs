using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AoE Damage Skill", menuName = "DungeonDefence/Skill/AoE Damage")]
public class AoeDamageSkillDataSO : SkillDataSO
{
    
    public float damageAmount = 60f;

    public override void Cast(Unit caster, Unit mainTarget)
    {
        var enemies = GetEnemies(caster);

        if (enemies.Count == 0)
        {
            
            return;
        }

        foreach (var enemy in enemies)
        {
            
            UnitManager.Instance.DamageUnit(enemy, damageAmount);
        }
    }
}



