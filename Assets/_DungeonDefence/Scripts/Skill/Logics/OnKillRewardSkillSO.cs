using UnityEngine;

/// <summary>
/// 유닛이 적을 처치했을 때 보상을 주는 스킬. (Vampire 전용)
/// </summary>
[CreateAssetMenu(fileName = "Skill_OnKillReward", menuName = "DungeonDefence/Skills/On Kill Reward")]
public class OnKillRewardSkillSO : SkillDataSO
{
    [Header("Reward Settings")]
    public float healAmount = 50f;
    public int bonusGold = 10;

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitKill(Unit owner, Unit victim)
    {
        if (owner == null) return;

        // 체력 회복
        owner.Combat.Heal(healAmount);
        Debug.Log($"<color=lime>[Reward] {owner.Data.Name} killed {victim.Data.Name}! Healed {healAmount}</color>");

        // 골드 보너스 (ResourceManager가 있다면 호출)
        // if (ResourceManager.Instance != null)
        //     ResourceManager.Instance.AddResource(CurrencyType.Gold, bonusGold);
        
        Debug.Log($"<color=yellow>[Reward] Bonus Gold: {bonusGold}</color>");
    }
}
