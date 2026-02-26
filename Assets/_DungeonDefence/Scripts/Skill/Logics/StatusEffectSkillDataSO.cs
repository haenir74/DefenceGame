using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum StatusEffectType
{
    Venom,
    Weakness,
    Corrosion
}

[CreateAssetMenu(fileName = "Skill_StatusEffect", menuName = "DungeonDefence/Skills/Status Effect")]
public class StatusEffectSkillDataSO : SkillDataSO
{

    public StatusEffectType effectType;
    public float duration = 5f;
    public float value = 10f;
    public bool isAoE = false;

    public override void Cast(Unit caster, Unit target)
    {
        if (isAoE)
        {
            var enemies = GetEnemies(caster);
            foreach (var enemy in enemies)
                Apply(enemy);
        }
        else if (target != null)
        {
            Apply(target);
        }
    }

    public void ApplyStatusEffect(Unit target, Unit attacker, float customDuration, float customValue)
    {
        if (target == null || target.IsDead) return;
        UnitManager.Instance.StartCoroutine(CustomEffectCoroutine(target, customDuration, customValue));
    }

    private IEnumerator CustomEffectCoroutine(Unit target, float customDuration, float customValue)
    {
        float elapsed = 0f;
        float originalAttackMult = target.Combat.AttackMultiplier;
        if (effectType == StatusEffectType.Weakness)
        {
            target.Combat.AttackMultiplier *= (1f - (customValue / 100f));
        }

        while (elapsed < customDuration && target != null && !target.IsDead)
        {
            if (effectType == StatusEffectType.Venom || effectType == StatusEffectType.Corrosion)
            {
                target.Combat.TakeDamage(customValue * Time.deltaTime);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (target != null && !target.IsDead)
        {
            if (effectType == StatusEffectType.Weakness)
                target.Combat.AttackMultiplier = originalAttackMult;
        }
    }

    private void Apply(Unit target)
    {
        if (target == null || target.IsDead) return;
        UnitManager.Instance.StartCoroutine(EffectCoroutine(target));
    }

    private IEnumerator EffectCoroutine(Unit target)
    {
        float elapsed = 0f;


        float originalAttackMult = target.Combat.AttackMultiplier;
        if (effectType == StatusEffectType.Weakness)
        {
            target.Combat.AttackMultiplier *= (1f - (value / 100f));
        }

        while (elapsed < duration && target != null && !target.IsDead)
        {
            if (effectType == StatusEffectType.Venom || effectType == StatusEffectType.Corrosion)
            {
                target.Combat.TakeDamage(value * Time.deltaTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }


        if (target != null && !target.IsDead)
        {
            if (effectType == StatusEffectType.Weakness)
                target.Combat.AttackMultiplier = originalAttackMult;
        }
    }
}



