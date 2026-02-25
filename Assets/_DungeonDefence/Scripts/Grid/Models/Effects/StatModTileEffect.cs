using UnityEngine;

[CreateAssetMenu(fileName = "StatModEffect", menuName = "DungeonDefence/Effects/StatMod")]
public class StatModTileEffect : TileEffectDataSO
{
    [Header("Ally Modifiers")]
    public float allySpeedMod = 1.0f;
    public float allyAttackMod = 1.0f;
    public bool allyRoot = false;

    [Header("Enemy Modifiers")]
    public float enemySpeedMod = 1.0f;
    public float enemyAttackMod = 1.0f;
    public bool enemyRoot = false;

    [Header("Category Filters")]
    public UnitCategory targetCategory = UnitCategory.None; // None = All

    public override void OnEnter(Unit unit)
    {
        Apply(unit, true);
    }

    public override void OnExit(Unit unit)
    {
        Apply(unit, false);
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
            unit.Combat.AttackMultiplier = attackMod;
        }
    }
}
