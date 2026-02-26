using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum StatusEffectType
{
    Venom,      // DoT
    Weakness,   // Atk/Aspd reduction
    Corrosion   // Def reduction or more Dmg taken
}

/// <summary>
/// 적에게 상태 이상을 부여하는 스킬. (Spider, Succubus 등 사용)
/// </summary>
[CreateAssetMenu(fileName = "Skill_StatusEffect", menuName = "DungeonDefence/Skills/Status Effect")]
public class StatusEffectSkillDataSO : SkillDataSO
{
    [Header("Effect Settings")]
    public StatusEffectType effectType;
    public float duration = 5f;
    public float value = 10f; // Dmg per sec or Reduction amount
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

    private void Apply(Unit target)
    {
        if (target == null || target.IsDead) return;
        UnitManager.Instance.StartCoroutine(EffectCoroutine(target));
    }

    private IEnumerator EffectCoroutine(Unit target)
    {
        float elapsed = 0f;
        
        // Initial Effect
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

        // Restore Stats
        if (target != null && !target.IsDead)
        {
            if (effectType == StatusEffectType.Weakness)
                target.Combat.AttackMultiplier = originalAttackMult;
        }
    }
}
