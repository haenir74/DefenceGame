using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Damage Skill", menuName = "DungeonDefence/Skill/Damage")]
public class DamageSkillDataSO : SkillDataSO
{
    [Header("Damage Settings")]
    public float damageAmount = 50f;

    public override void Cast(Unit caster, Unit mainTarget)
    {
        if (mainTarget != null)
        {
            Debug.Log($"<color=red>[Skill] {caster.name} >> {skillName} >> {mainTarget.name} (Dmg: {damageAmount})</color>");
            UnitManager.Instance.DamageUnit(mainTarget, damageAmount);
        }
    }
}