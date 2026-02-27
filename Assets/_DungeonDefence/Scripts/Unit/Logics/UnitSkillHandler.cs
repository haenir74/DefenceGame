using UnityEngine;

public class UnitSkillHandler : MonoBehaviour
{
    private Unit unit;

    public void Initialize(Unit unit)
    {
        this.unit = unit;

        if (unit.Combat != null)
        {
            unit.Combat.OnAttackHit -= HandleAttackHit;
            unit.Combat.OnAttackHit += HandleAttackHit;

            unit.Combat.OnMaxMpReached -= HandleMaxMpReached;
            unit.Combat.OnMaxMpReached += HandleMaxMpReached;
        }
    }

    private void HandleAttackHit(Unit target, float damage)
    {
        if (unit == null || unit.IsDead || unit.Data == null) return;

        bool hasSkill = unit.Data.skill != null && unit.Data.maxMp > 0;
        if (hasSkill)
        {
            unit.Combat.AddMp(10f * unit.Combat.MpMultiplier);
        }
    }

    private void HandleMaxMpReached()
    {
        if (unit == null || unit.IsDead || unit.Data == null || unit.Data.skill == null) return;

        unit.Combat.ResetMp();

        Unit target = UnitManager.Instance.GetOpponentAt(unit.Coordinate, unit.IsPlayerTeam);
        unit.Data.skill.Cast(unit, target);
    }

    private void OnDestroy()
    {
        if (unit != null && unit.Combat != null)
        {
            unit.Combat.OnAttackHit -= HandleAttackHit;
            unit.Combat.OnMaxMpReached -= HandleMaxMpReached;
        }
    }
}
