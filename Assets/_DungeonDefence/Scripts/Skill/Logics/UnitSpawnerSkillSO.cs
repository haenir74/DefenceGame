using UnityEngine;

/// <summary>
/// 주기적으로 특정 유닛을 소환하는 스킬. (Slime Pool 등에서 사용)
/// </summary>
[CreateAssetMenu(fileName = "Skill_UnitSpawner", menuName = "DungeonDefence/Skills/Unit Spawner")]
public class UnitSpawnerSkillSO : SkillDataSO
{
    [Header("Spawn Settings")]
    public UnitDataSO unitToSpawn;
    public float spawnInterval = 5f;
    
    private Dictionary<Unit, float> spawnTimers = new Dictionary<Unit, float>();

    public override void Cast(Unit caster, Unit target) { }

    public override void OnUnitUpdate(Unit owner)
    {
        if (unitToSpawn == null) return;

        if (!spawnTimers.ContainsKey(owner))
            spawnTimers[owner] = 0f;

        spawnTimers[owner] += Time.deltaTime;

        if (spawnTimers[owner] >= spawnInterval)
        {
            spawnTimers[owner] = 0f;
            TrySpawn(owner);
        }
    }

    private void TrySpawn(Unit owner)
    {
        if (owner.CurrentNode == null) return;
        
        Unit spawned = UnitManager.Instance.SpawnUnit(unitToSpawn, owner.CurrentNode);
        if (spawned != null)
        {
            Debug.Log($"<color=cyan>[Spawner] {owner.Data.Name} spawned {unitToSpawn.Name}</color>");
        }
    }
}
