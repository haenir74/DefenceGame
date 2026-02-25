using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 사망 시 1회 부활하는 스킬. (Lich 전용)
/// </summary>
[CreateAssetMenu(fileName = "Skill_Resurrection", menuName = "DungeonDefence/Skills/Resurrection")]
public class ResurrectionSkillSO : SkillDataSO
{
    [Header("Resurrection Settings")]
    public float healHealthRatio = 1.0f; // 100% HP
    
    // 웨이브마다 초기화되어야 하므로, 리스트로 관리 (실제는 Buff 등으로 관리 권장)
    private HashSet<Unit> resurrectedUnits = new HashSet<Unit>();

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitDie(Unit owner)
    {
        if (owner == null) return;

        if (!resurrectedUnits.Contains(owner))
        {
            resurrectedUnits.Add(owner);
            
            // 부활 로직: 사망 상태 해제 및 체력 회복
            // Unit.HandleDeath가 이미 진행 중이므로, 
            // 여기서는 단순 수치 보정보다는 '사망 방지'가 더 적합함.
            // 하지만 시스템 구조상 HandleDeath를 막을 수 없으므로, 
            // '즉시 재소환' 형태로 처리함.
            
            UnitManager.Instance.SpawnUnit(owner.Data, owner.CurrentNode);
            Debug.Log($"<color=white>[Resurrect] {owner.Data.Name} has resurrected!</color>");
        }
    }
    
    // 웨이브 클리어 시 초기화 (외부에서 호출 필요)
    public void Reset()
    {
        resurrectedUnits.Clear();
    }
}
