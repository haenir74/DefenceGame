using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxEnemies = 10;

    private float timer;
    private int spawnedCount;

    void Update()
    {
        // 간단한 웨이브 로직: 일정 시간마다 최대 갯수까지 생성
        if (spawnedCount < maxEnemies)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnEnemy();
                timer = 0;
            }
        }
    }

    private void SpawnEnemy()
    {
        if (GameManager.Instance == null) return;
        
        MapContext map = GameManager.Instance.Context.map;
        if (map == null || map.SpawnNode == null)
        {
            Debug.LogWarning("Spawn Node is not defined!");
            return;
        }

        Vector3 spawnPos = map.SpawnNode.WorldPosition;
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        EnemyUnit enemyScript = enemyObj.GetComponent<EnemyUnit>();
        if (enemyScript != null)
        {
            enemyScript.SetNode(map.SpawnNode);
            // EnemyUnit.Start()에서 자동으로 경로를 찾고 이동 시작함
        }

        spawnedCount++;
    }
}
