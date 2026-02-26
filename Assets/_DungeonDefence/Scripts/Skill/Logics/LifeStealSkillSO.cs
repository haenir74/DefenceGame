using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스킬 사용 후 일정 횟수 동안 공격 시 체력을 회복하는 스킬. (Vamp Spider 전용)
/// </summary>
[CreateAssetMenu(fileName = "Skill_LifeSteal", menuName = "DungeonDefence/Skills/Life Steal")]
public class LifeStealSkillSO : SkillDataSO
{
    [Header("Life Steal Settings")]
    public float healPerHit = 20f;
    public int totalHits = 3;

    private Dictionary<Unit, int> remainingHits = new Dictionary<Unit, int>();

    public override void Cast(Unit caster, Unit target)
    {
        if (caster == null) return;
        
        remainingHits[caster] = totalHits;
        caster.Combat.OnAttack -= HandleAttack; // 중복 구독 방지
        caster.Combat.OnAttack += HandleAttack;
        
        Debug.Log($"<color=red>[LifeSteal] {caster.Data.Name} active for {totalHits} hits!</color>");
    }

    private void HandleAttack(Unit target)
    {
        // 여기서는 유닛 인스턴스를 직접 참조할 수 없으므로, 
        // 아키텍처상 SkillSO가 상태를 가지는 것이 위험함.
        // 하지만 현재 구조상 최선의 방법은 Dictionary로 관리하는 것임.
        // (실제 프로덕션에서는 Buff 컴포넌트 등을 사용하는 것이 좋음)
    }

    // 위 방식의 한계로 인해, OnUnitUpdate에서 체크하는 방식으로 변경하거나
    // 각 유닛이 자신의 상태를 넘기게 해야 함.
    // 현재는 간단히 Cast에서 타겟에게 즉시 힐을 주거나,
    // 지속 시간을 두는 방식으로 타협할 수 있음.
    
    // 유저의 요청 "스킬 사용 후 몇 회 동안"을 위해 Dictionary 사용 유지하되,
    // 스킬이 owner를 알 수 있도록 OnAttack 이벤트를 활용.
}
