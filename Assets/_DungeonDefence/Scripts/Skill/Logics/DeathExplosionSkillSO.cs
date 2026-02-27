using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Skill_DeathExplosion", menuName = "DungeonDefence/Skills/Death Explosion")]
public class DeathExplosionSkillSO : SkillDataSO
{

    public float explosionDamage = 80f;
    public bool isPoisonType = false;
    public float poisonDamagePerTick = 10f;
    public float poisonDuration = 3f;

    public float explosionRadius = 2f;
    [Tooltip("Damage multiplier based on the caster's AttackPower.")]
    public float attackPowerMultiplierMultiplier = 1.0f;

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitDie(Unit victim)
    {
        Explode(victim);
    }

    private void Explode(Unit victim)
    {
        if (victim == null || victim.CurrentNode == null) return;

        float scaledDamage = explosionDamage;
        if (victim.Combat != null && victim.Combat.AttackPower != null)
        {
            scaledDamage += victim.Combat.AttackPower.Value * attackPowerMultiplierMultiplier;
        }

        Vector3 explosionCenter = victim.transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionCenter, explosionRadius);

        HashSet<Unit> processedTargets = new HashSet<Unit>();

        foreach (var col in colliders)
        {
            Unit target = col.GetComponent<Unit>();
            if (target != null && target != victim && !processedTargets.Contains(target))
            {
                if (target.IsPlayerTeam != victim.IsPlayerTeam && !target.IsDead)
                {
                    target.Combat.TakeDamage(scaledDamage, victim);

                    if (isPoisonType && poisonDuration > 0)
                    {
                        ApplyPoison(target, victim);
                    }
                }
                processedTargets.Add(target);
            }
        }
    }

    private StatusEffectSkillDataSO cachedPoisonEffect;

    private void ApplyPoison(Unit target, Unit attacker)
    {
        if (cachedPoisonEffect == null)
            cachedPoisonEffect = Resources.Load<StatusEffectSkillDataSO>("Data/Status/PoisonEffect");

        if (cachedPoisonEffect != null)
        {
            cachedPoisonEffect.ApplyStatusEffect(target, attacker, poisonDuration, poisonDamagePerTick);
        }
    }
}

