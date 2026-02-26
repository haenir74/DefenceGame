using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 사망 시 발동: 같은 타일에 노멀 슬라임(normalSlimeData)을 splitCount마리 소환.
/// 거대 슬라임(big_slime) 전용.
/// </summary>
[CreateAssetMenu(fileName = "Skill_DeathSplit", menuName = "DungeonDefence/Skills/Death Split")]
public class DeathSplitSkillSO : SkillDataSO
{
    [Header("Split Settings")]
    [Tooltip("분열 시 소환할 슬라임 UnitDataSO (노멀 슬라임)")]
    public UnitDataSO normalSlimeData;
    [Tooltip("소환 마리 수 (최소~최대 랜덤)")]
    public int splitCountMin = 4;
    public int splitCountMax = 5;

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitDie(Unit owner)
    {
        if (owner == null || owner.CurrentNode == null || normalSlimeData == null)
        {
            Debug.LogWarning("[DeathSplit] normalSlimeData\uAC00 null\uC774\uAC70\uB098 \uB178\uB4DC \uC815\uBCF4 \uC5C6\uC74C");
            return;
        }

        int count = UnityEngine.Random.Range(splitCountMin, splitCountMax + 1);
        int spawned = 0;

        for (int i = 0; i < count; i++)
        {
            // 같은 노드의 빈 슬롯에 소환 (슬롯 없으면 중단)
            Unit newSlime = UnitManager.Instance.SpawnUnit(normalSlimeData, owner.CurrentNode);
            if (newSlime != null)
            {
                spawned++;
                Debug.Log($"<color=lime>[DeathSplit] \uBD84\uC5F4! \uC2AC\uB77C\uC784 \uC18C\uD658 {spawned}/{count}</color>");
            }
            else
            {
                Debug.Log("[DeathSplit] \uC21C\uAC04 \uc2ac\ub86f\uc774 \uaf49 \uccd0\uc2b5\ub2c8\ub2e4. \uc18c\ud658 \uc911\ub2e8.");
                break;
            }
        }
    }
}
