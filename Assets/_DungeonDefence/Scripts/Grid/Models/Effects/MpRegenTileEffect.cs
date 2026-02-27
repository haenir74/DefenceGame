using UnityEngine;

[CreateAssetMenu(fileName = "MpRegenEffect", menuName = "DungeonDefence/Effects/MpRegen")]
public class MpRegenTileEffect : TileEffectDataSO
{
    public float multiplier = 3.0f;

    public override void ApplyEffect(Unit targetUnit, int currentStacks)
    {
        if (targetUnit != null && targetUnit.Combat != null)
        {
            targetUnit.Combat.MpMultiplier = multiplier;
        }
    }

    public override void RemoveEffect(Unit targetUnit, int currentStacks)
    {
        if (targetUnit != null && targetUnit.Combat != null)
        {
            targetUnit.Combat.MpMultiplier = 1.0f;
        }
    }
}



