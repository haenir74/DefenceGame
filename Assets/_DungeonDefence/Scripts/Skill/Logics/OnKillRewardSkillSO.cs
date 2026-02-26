using UnityEngine;

[CreateAssetMenu(fileName = "Skill_OnKillReward", menuName = "DungeonDefence/Skills/On Kill Reward")]
public class OnKillRewardSkillSO : SkillDataSO
{
    
    public float healAmount = 50f;
    public int bonusGold = 10;

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitKill(Unit owner, Unit victim)
    {
        if (owner == null) return;

        
        owner.Combat.Heal(healAmount);
        

        
        
        
        
        
    }
}



