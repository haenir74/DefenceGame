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

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitDie(Unit victim)
    {
        Explode(victim);
    }

    private void Explode(Unit victim)
    {
        if (victim == null || victim.CurrentNode == null) return;

        List<Unit> targets = UnitManager.Instance.GetUnitsOnNode(victim.CurrentNode);
        foreach (var target in targets)
        {
            if (target != null && target.IsPlayerTeam != victim.IsPlayerTeam && !target.IsDead)
            {
                target.Combat.TakeDamage(explosionDamage, victim);

                if (isPoisonType && poisonDuration > 0)
                {
                    ApplyPoison(target, victim);
                }
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

