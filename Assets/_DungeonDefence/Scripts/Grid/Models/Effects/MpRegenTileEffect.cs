using UnityEngine;

[CreateAssetMenu(fileName = "MpRegenEffect", menuName = "DungeonDefence/Effects/MpRegen")]
public class MpRegenTileEffect : TileEffectDataSO
{
    public float multiplier = 3.0f; // 200% increase means 3.0 total

    public override void OnEnter(Unit unit)
    {
        if (unit != null && unit.Combat != null)
        {
            unit.Combat.MpMultiplier = multiplier;
        }
    }

    public override void OnExit(Unit unit)
    {
        if (unit != null && unit.Combat != null)
        {
            unit.Combat.MpMultiplier = 1.0f;
        }
    }
}
