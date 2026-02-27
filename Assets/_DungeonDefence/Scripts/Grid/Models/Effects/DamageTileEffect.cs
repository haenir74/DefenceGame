using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "DungeonDefence/Effects/Damage")]
public class DamageTileEffect : TileEffectDataSO
{
    public float enterDamage = 0f;
    public float dotDamage = 0f;
    public bool targetEnemies = true;
    public bool targetAllies = false;

    public override void ApplyEffect(Unit targetUnit, int currentStacks)
    {
        if (ShouldApply(targetUnit) && enterDamage > 0)
        {
            targetUnit.Combat.TakeDamage(enterDamage);
        }

        if (ShouldApply(targetUnit) && dotDamage > 0)
        {
            targetUnit.Combat.TakeDamage(dotDamage);
        }
    }

    public override void RemoveEffect(Unit targetUnit, int currentStacks)
    {

    }

    private bool ShouldApply(Unit unit)
    {
        if (unit == null || unit.IsDead) return false;
        if (unit.IsPlayerTeam && targetAllies) return true;
        if (!unit.IsPlayerTeam && targetEnemies) return true;
        return false;
    }
}



