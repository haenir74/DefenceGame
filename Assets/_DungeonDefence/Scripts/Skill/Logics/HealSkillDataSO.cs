using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Heal Skill", menuName = "DungeonDefence/Skill/Heal")]
public class HealSkillDataSO : SkillDataSO
{
    [Header("Heal Settings")]
    public float healAmount = 30f;

    public override void Cast(Unit caster, Unit mainTarget)
    {
        var allies = GetAllies(caster);

        var targetAlly = allies
            .OrderBy(u => u.Combat.CurrentHp / u.Combat.MaxHp)
            .FirstOrDefault();

        if (targetAlly != null)
        {
            Debug.Log($"<color=green>[Skill] {caster.name} >> {skillName} >> {targetAlly.name} (Heal: {healAmount})</color>");
            targetAlly.Combat.TakeDamage(-healAmount);
        }
        else
        {
            Debug.Log($"<color=yellow>[Skill] {caster.name}: 치유할 대상이 없습니다.</color>");
        }
    }
}