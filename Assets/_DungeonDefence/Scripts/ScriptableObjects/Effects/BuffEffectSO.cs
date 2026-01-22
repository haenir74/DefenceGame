using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Effects/Buff Effect")]
public class BuffEffectSO : TileEffectSO
{
    public float buffAmount = 5f;
    // public StatType targetStat; // 나중에 StatType Enum 필요

    public override void ExecuteEnter(Unit unit)
    {
        if (unit.MyTeam == Team.Ally)
        {
            Debug.Log($"{unit.name} 버프 적용: +{buffAmount}");
            // unit.Stats.AddBonus(targetStat, buffAmount); 
        }
    }

    public override void ExecuteExit(Unit unit)
    {
        if (unit.MyTeam == Team.Ally)
        {
            Debug.Log($"{unit.name} 버프 해제");
            // unit.Stats.RemoveBonus(targetStat, buffAmount);
        }
    }
}