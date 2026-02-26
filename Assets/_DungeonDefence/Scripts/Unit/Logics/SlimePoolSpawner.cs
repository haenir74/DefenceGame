using UnityEngine;

public class SlimePoolSpawner : MonoBehaviour
{
    [SerializeField] private UnitDataSO normalSlimeData;
    [SerializeField] private float spawnInterval = 6f;

    private Unit owner;
    private float timer;

    private void Awake()
    {
        owner = GetComponent<Unit>();
    }

    private void OnEnable()
    {
        timer = spawnInterval; 
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

        if (!owner.CurrentNode.HasFreeSlot(true))
        {
            return;
        }

        UnitManager.Instance.SpawnUnit(normalSlimeData, owner.CurrentNode);
    }
}

