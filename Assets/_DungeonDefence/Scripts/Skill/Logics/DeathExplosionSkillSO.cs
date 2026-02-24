using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 사망 시 발동: 같은 타일의 모든 적에게 피해를 주고, 선택 시 독 상태이상(지속 피해)을 부여.
/// 맹독 슬라임(green)과 폭발 슬라임(red) 공용. isPoisonType으로 독/폭발 구분.
/// </summary>
[CreateAssetMenu(fileName = "Skill_DeathExplosion", menuName = "DungeonDefence/Skills/Death Explosion")]
public class DeathExplosionSkillSO : SkillDataSO
{
    [Header("Explosion Settings")]
    [Tooltip("폭발 피해량")]
    public float explosionDamage = 80f;
    [Tooltip("독 타입이면 true, 폭발 타입이면 false")]
    public bool isPoisonType = false;
    [Tooltip("독 타입 시 독 지속 피해 (3초간)")]
    public float poisonDamagePerTick = 10f;
    [Tooltip("독 지속 시간 (초)")]
    public float poisonDuration = 3f;

    // Cast는 전투 중 스킬로는 사용하지 않음
    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitDie(Unit owner)
    {
        if (owner == null || owner.CurrentNode == null) return;

        List<Unit> enemies = GetEnemies(owner);
        foreach (Unit enemy in enemies)
        {
            enemy.Combat.TakeDamage(explosionDamage);
            Debug.Log($"<color=red>[DeathExplosion] {owner.Data.unitName} \uc790\ud3ed! {enemy.Data.unitName}\uc5d0\uac8c {explosionDamage} \ud53c\ud574</color>");

            if (isPoisonType)
            {
                ApplyPoison(enemy);
            }
        }
    }

    private void ApplyPoison(Unit target)
    {
        if (target == null || target.IsDead) return;
        // PoisonEffect가 없으면 UnitManager 코루틴으로 지속 피해 처리
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.StartCoroutine(PoisonCoroutine(target));
        }
    }

    private System.Collections.IEnumerator PoisonCoroutine(Unit target)
    {
        float elapsed = 0f;
        float tick = 1f;
        float tickTimer = 0f;

        while (elapsed < poisonDuration && target != null && !target.IsDead)
        {
            tickTimer += UnityEngine.Time.deltaTime;
            elapsed += UnityEngine.Time.deltaTime;

            if (tickTimer >= tick)
            {
                tickTimer = 0f;
                target.Combat.TakeDamage(poisonDamagePerTick);
                Debug.Log($"[Poison] {target.Data.Name} 독 피해: {poisonDamagePerTick}");
            }
            yield return null;
        }
    }
}
