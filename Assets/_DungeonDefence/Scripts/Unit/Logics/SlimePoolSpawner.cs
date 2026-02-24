using UnityEngine;

/// <summary>
/// 슬라임 풀 전용 컴포넌트: 일정 주기마다 같은 노드에 노멀 슬라임을 소환.
/// UnitDataSO.category = Tower로 설정하고 이 컴포넌트를 프리팹에 추가.
/// </summary>
public class SlimePoolSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private UnitDataSO normalSlimeData;
    [Tooltip("소환 주기 (초)")]
    [SerializeField] private float spawnInterval = 6f;

    private Unit owner;
    private float timer;

    private void Awake()
    {
        owner = GetComponent<Unit>();
    }

    private void OnEnable()
    {
        timer = spawnInterval; // 첫 소환은 1주기 후
    }

    private void Update()
    {
        if (owner == null || owner.IsDead || !owner.IsPlayerTeam) return;
        if (normalSlimeData == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = spawnInterval;
            SpawnSlime();
        }
    }

    private void SpawnSlime()
    {
        if (owner.CurrentNode == null) return;

        // 빈 슬롯이 있을 때만 소환
        if (!owner.CurrentNode.HasFreeSlot(true))
        {
            Debug.Log("[SlimePool] 슬롯 가득 참. 소환 스킵.");
            return;
        }

        Unit slime = UnitManager.Instance.SpawnUnit(normalSlimeData, owner.CurrentNode);
        if (slime != null)
            Debug.Log($"<color=cyan>[SlimePool] 슬라임 소환!</color>");
    }
}
