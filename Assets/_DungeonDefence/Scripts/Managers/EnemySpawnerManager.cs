using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerManager : Singleton<EnemySpawnerManager>
{
    [Header("Settings")]
    [SerializeField] private List<UnitDataSO> enemySpawnList;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxEnemies = 10;

    [SerializeField] private bool isSpawning = true;

    private float timer;
    private int spawnedCount;

    private MapContext Map => GameManager.Instance != null ? GameManager.Instance.Context.map : null;


    protected override void Awake()
    {
        base.Awake();
        spawnedCount = 0;
        timer = spawnInterval;
    }

    void Update()
    {
        if (!isSpawning || GameManager.Instance == null) return;
        if (spawnedCount >= maxEnemies) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0;
        }
    }

    private void SpawnEnemy()
    {
        if (GameManager.Instance == null) return;
        
        if (enemySpawnList == null || enemySpawnList.Count == 0)
        {
            isSpawning = false;
            return;
        }

        if (Map == null || Map.SpawnNode == null) return;

        UnitDataSO enemyData = enemySpawnList[Random.Range(0, enemySpawnList.Count)];
        if (enemyData == null || enemyData.prefab == null) return;

        Vector3 spawnPos = Map.SpawnNode.WorldPosition;
        GameObject enemyObj = Instantiate(enemyData.prefab, spawnPos, Quaternion.identity);

        Unit enemyScript = enemyObj.GetComponent<Unit>();
        if (enemyScript != null)
        {
            enemyScript.InitializeUnit(enemyData);
            enemyScript.SetNode(Map.SpawnNode);
        }
        else
        {
            Destroy(enemyObj);
            return;
        }

        spawnedCount++;
    }

    public void ResetSpawner()
    {
        spawnedCount = 0;
        timer = 0;
        isSpawning = true;
    }
}
