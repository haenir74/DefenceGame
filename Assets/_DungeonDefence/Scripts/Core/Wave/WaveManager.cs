using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WaveManager : Singleton<WaveManager>
{
    [Header("Settings")]
    [SerializeField] private List<WaveDataSO> waves;

    // WaveIndex, Remaining, Total
    public event Action<int, int, int> OnWaveInfoChanged;
    public event Action OnWaveCompleted;

    private int currentWaveIndex = 0;
    private int totalEnemiesInCurrentWave;
    private int aliveEnemiesCount;
    private bool isWaveInProgress;
    private Coroutine spawnCoroutine;

    private void Start()
    {
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnUnitDead += HandleUnitDead;
        }
    }

    public void StartWave(int waveIndex)
    {
        if (waves == null || waveIndex < 0 || waveIndex >= waves.Count)
        {
            OnWaveCompleted?.Invoke();
            return;
        }

        WaveDataSO waveData = waves[waveIndex];
        this.currentWaveIndex = waveIndex + 1;
        this.totalEnemiesInCurrentWave = waveData.GetTotalEnemyCount();
        this.aliveEnemiesCount = this.totalEnemiesInCurrentWave;
        this.isWaveInProgress = true;
        
        NotifyUI();

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine(waveData));
    }

    private IEnumerator SpawnRoutine(WaveDataSO data)
    {
        GridNode spawnNode = GridManager.Instance.GetSpawnNode();
        if (spawnNode == null) yield break;

        foreach (var group in data.groups)
        {
            if (group.initialDelay > 0)
                yield return new WaitForSeconds(group.initialDelay);

            for (int i = 0; i < group.count; i++)
            {
                UnitManager.Instance.SpawnUnit(group.unitData, spawnNode);
                yield return new WaitForSeconds(group.spawnInterval);
            }
        }
    }

    private void HandleUnitDead(Unit unit)
    {
        if (!isWaveInProgress || unit.IsPlayerTeam) return;

        aliveEnemiesCount--;
        if (aliveEnemiesCount < 0) aliveEnemiesCount = 0;

        NotifyUI();

        if (aliveEnemiesCount == 0)
        {
            FinishWave();
        }
    }

    private void FinishWave()
    {
        isWaveInProgress = false;
        Debug.Log($"[WaveManager] 웨이브 {currentWaveIndex} 클리어!");
        OnWaveCompleted?.Invoke();
    }

    private void NotifyUI()
    {
        OnWaveInfoChanged?.Invoke(currentWaveIndex, aliveEnemiesCount, totalEnemiesInCurrentWave);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (UnitManager.Instance != null)
            UnitManager.Instance.OnUnitDead -= HandleUnitDead;
    }
}