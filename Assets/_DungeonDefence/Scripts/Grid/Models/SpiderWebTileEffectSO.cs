using UnityEngine;

[CreateAssetMenu(fileName = "New SpiderWeb Tile Effect", menuName = "DungeonDefence/Effects/Tile/SpiderWeb")]
public class SpiderWebTileEffectSO : TileEffectDataSO
{
    [Header("SpiderWeb Settings")]
    public float attackPowerBonusPercent = 0.2f;

    public override void ApplyEffect(Unit target, int currentStacks)
    {
        if (target != null && target.Data != null)
        {
            if (target.Data.HasTag(UnitTag.Spider))
            {
                if (target.Combat != null && target.Combat.AttackPower != null)
                {
                    StatModifier mod = new StatModifier(attackPowerBonusPercent, StatModType.PercentMultiply, this);
                    target.Combat.AttackPower.AddModifier(mod);
                }
            }
        }
    }

    public override void RemoveEffect(Unit target, int currentStacks)
    {
        if (target != null && target.Combat != null && target.Combat.AttackPower != null)
        {
            target.Combat.AttackPower.RemoveAllModifiersFromSource(this);
        }
    }
}
