using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Skill_DeathSplit", menuName = "DungeonDefence/Skills/Death Split")]
public class DeathSplitSkillSO : SkillDataSO
{
    
    
    public UnitDataSO normalSlimeData;
    
    public int splitCountMin = 4;
    public int splitCountMax = 5;

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitDie(Unit owner)
    {
        if (owner == null || owner.CurrentNode == null || normalSlimeData == null)
        {
            
            return;
        }

        int count = UnityEngine.Random.Range(splitCountMin, splitCountMax + 1);
        int spawned = 0;

        for (int i = 0; i < count; i++)
        {
            
            Unit newSlime = UnitManager.Instance.SpawnUnit(normalSlimeData, owner.CurrentNode);
            if (newSlime != null)
            {
                spawned++;
                
            }
            else
            {
                
                break;
            }
        }
    }
}



