using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Skill_LifeSteal", menuName = "DungeonDefence/Skills/Life Steal")]
public class LifeStealSkillSO : SkillDataSO
{
    
    public float healPerHit = 20f;
    public int totalHits = 3;

    private Dictionary<Unit, int> remainingHits = new Dictionary<Unit, int>();

    public override void Cast(Unit caster, Unit target)
    {
        if (caster == null) return;
        
        remainingHits[caster] = totalHits;
        caster.Combat.OnAttack -= HandleAttack; 
        caster.Combat.OnAttack += HandleAttack;
        
        
    }

    private void HandleAttack(Unit target)
    {
        
        
        
        
    }

    
    
    
    
    
    
    
}



