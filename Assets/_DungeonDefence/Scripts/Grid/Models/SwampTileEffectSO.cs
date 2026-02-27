using UnityEngine;

[CreateAssetMenu(fileName = "New Swamp Tile Effect", menuName = "DungeonDefence/Effects/Tile/Swamp")]
public class SwampTileEffectSO : TileEffectDataSO
{
    [Header("Swamp Refund Setting")]
    [Tooltip("Refund ratio when a Slime dies on this tile.")]
    public float refundRatio = 0.5f;

    public override void ApplyEffect(Unit target, int currentStacks)
    {
        // No stats modified upon entry for this specific effect.
    }

    public override void RemoveEffect(Unit target, int currentStacks)
    {
        // No stats to remove upon exit.
    }

    public override void ExecuteDeathEffect(Unit target, int currentStacks)
    {
        if (target != null && target.Data != null && target.Data.HasTag(UnitTag.Slime))
        {
            var costs = target.Data.GetCosts();
            if (costs != null && EconomyManager.InstanceExists)
            {
                foreach (var cost in costs)
                {
                    int refundAmount = Mathf.FloorToInt(cost.amount * refundRatio);
                    if (refundAmount > 0)
                    {
                        EconomyManager.Instance.AddCurrency(cost.type, refundAmount);
                    }
                }
            }
        }
    }
}
