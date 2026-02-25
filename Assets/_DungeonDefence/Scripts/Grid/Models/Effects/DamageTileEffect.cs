using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "DungeonDefence/Effects/Damage")]
public class DamageTileEffect : TileEffectDataSO
{
    public float enterDamage = 0f;
    public float dotDamage = 0f;
    public bool targetEnemies = true;
    public bool targetAllies = false;

    public override void OnEnter(Unit unit)
    {
        if (ShouldApply(unit) && enterDamage > 0)
        {
            unit.Combat.TakeDamage(enterDamage);
        }
    }

    public override void OnUpdate(Unit unit)
    {
        if (ShouldApply(unit) && dotDamage > 0)
        {
            unit.Combat.TakeDamage(dotDamage * Time.deltaTime);
        }
    }

    private bool ShouldApply(Unit unit)
    {
        if (unit == null || unit.IsDead) return false;
        if (unit.IsPlayerTeam && targetAllies) return true;
        if (!unit.IsPlayerTeam && targetEnemies) return true;
        return false;
    }
}
