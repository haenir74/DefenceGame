using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 모든 타일에 있는 특정 카테고리 유닛들에게 버프를 부여하는 패시브 스킬. (Spider Queen 전용)
/// </summary>
[CreateAssetMenu(fileName = "Skill_GlobalBuff", menuName = "DungeonDefence/Skills/Global Buff")]
public class GlobalBuffSkillSO : SkillDataSO
{
    [Header("Buff Settings")]
    public UnitCategory targetCategory = UnitCategory.Spider;
    public float attackPowerMultiplier = 1.2f;

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitUpdate(Unit owner)
    {
        // 최적화를 위해 매 프레임이 아닌 일정 간격으로 적용할 수도 있지만, 
        // 현재는 간단히 배율만 덮어씌움 (중첩 방지 등은 구현에 따라 복잡해질 수 있음)
        if (UnitManager.Instance == null) return;

        var allUnits = UnitManager.Instance.GetAllUnits();
        foreach (var unit in allUnits)
        {
            if (unit.IsPlayerTeam == owner.IsPlayerTeam && unit.Data != null && unit.Data.category == targetCategory)
            {
                // UnitCombat에 배율 적용 프로퍼티가 있다고 가정하거나 직접 설정
                unit.Combat.AttackMultiplier = attackPowerMultiplier;
            }
        }
    }
}
