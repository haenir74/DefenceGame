using UnityEngine;

[CreateAssetMenu(fileName = "StatModEffect", menuName = "DungeonDefence/Effects/StatMod")]
public class StatModTileEffect : TileEffectDataSO
{

    public float allySpeedMod = 1.0f;
    public float allyAttackMod = 1.0f;
    public bool allyRoot = false;


    public float enemySpeedMod = 1.0f;
    public float enemyAttackMod = 1.0f;
    public bool enemyRoot = false;


    public UnitCategory targetCategory = UnitCategory.None;

    public override void ApplyEffect(Unit targetUnit, int currentStacks)
    {
        Apply(targetUnit, true);
    }

    public override void RemoveEffect(Unit targetUnit, int currentStacks)
    {
        Apply(targetUnit, false);
    }

    private void Apply(Unit unit, bool isEnter)
    {
        if (unit == null || unit.IsDead) return;
        if (targetCategory != UnitCategory.None && unit.Data.category != targetCategory) return;

        bool isPlayer = unit.IsPlayerTeam;
        float speedMod = isPlayer ? allySpeedMod : enemySpeedMod;
        float attackMod = isPlayer ? allyAttackMod : enemyAttackMod;
        bool root = isPlayer ? allyRoot : enemyRoot;

        if (!isEnter)
        {
            speedMod = 1.0f;
            attackMod = 1.0f;
            root = false;
        }

        if (unit.Movement != null)
        {
            unit.Movement.SpeedMultiplier = speedMod;
            unit.Movement.IsRooted = root;
        }

        if (unit.Combat != null)
        {
            unit.Combat.AttackPower.RemoveAllModifiersFromSource(this);
            if (isEnter && attackMod != 1.0f)
            {
                unit.Combat.AttackPower.AddModifier(new StatModifier(attackMod - 1.0f, StatModType.PercentMultiply, this));
            }
        }
    }
}



