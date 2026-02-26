using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AoE Damage Skill", menuName = "DungeonDefence/Skill/AoE Damage")]
public class AoeDamageSkillDataSO : SkillDataSO
{
    [Header("AoE Damage Settings")]
    public float damageAmount = 60f;

    public override void Cast(Unit caster, Unit mainTarget)
    {
        var enemies = GetEnemies(caster);

        if (enemies.Count == 0)
        {
            Debug.Log($"<color=red>[Skill] {caster.name} >> {skillName}: 대상 없음</color>");
            return;
        }

        foreach (var enemy in enemies)
        {
            Debug.Log($"<color=red>[Skill] {caster.name} >> {skillName} >> {enemy.name} (AoE Dmg: {damageAmount})</color>");
            UnitManager.Instance.DamageUnit(enemy, damageAmount);
        }
    }
}
