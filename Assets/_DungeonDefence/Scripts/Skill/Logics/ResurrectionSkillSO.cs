using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Skill_Resurrection", menuName = "DungeonDefence/Skills/Resurrection")]
public class ResurrectionSkillSO : SkillDataSO
{
    
    public float healHealthRatio = 1.0f; 
    
    
    private HashSet<Unit> resurrectedUnits = new HashSet<Unit>();

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitDie(Unit owner)
    {
        if (owner == null) return;

        if (!resurrectedUnits.Contains(owner))
        {
            resurrectedUnits.Add(owner);
            
            
            
            
            
            
            
            UnitManager.Instance.SpawnUnit(owner.Data, owner.CurrentNode);
            
        }
    }
    
    
    public void Reset()
    {
        resurrectedUnits.Clear();
    }
}



