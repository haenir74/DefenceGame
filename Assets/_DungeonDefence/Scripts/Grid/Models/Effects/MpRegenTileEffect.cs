using UnityEngine;

[CreateAssetMenu(fileName = "MpRegenEffect", menuName = "DungeonDefence/Effects/MpRegen")]
public class MpRegenTileEffect : TileEffectDataSO
{
    public float multiplier = 3.0f;

    public override void ExecuteEnterEffect(Unit unit)
    {
        if (unit != null && unit.Combat != null)
        {
            unit.Combat.MpMultiplier = multiplier;
        }
    }

    public override void ExecuteExitEffect(Unit unit)
    {
        if (unit != null && unit.Combat != null)
        {
            unit.Combat.MpMultiplier = 1.0f;
        }
    }
}



