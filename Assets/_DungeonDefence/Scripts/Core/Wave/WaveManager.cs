using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class WaveConfig
{
    public List<WaveDataSO> waveDatas;
}

public class WaveManager : Singleton<WaveManager>
{
    [Header("Settings")]
    [SerializeField] private List<WaveConfig> waves;

    // WaveIndex, Remaining, Total
    public event Action<int, int, int> OnWaveInfoChanged;
    public event Action OnWaveCompleted;

    private int currentWaveIndex = 0;
    private int totalEnemiesInCurrentWave;
    private int aliveEnemiesCount;
    private bool isWaveInProgress;
    private List<Coroutine> spawnCoroutines = new List<Coroutine>();

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

        WaveConfig config = waves[waveIndex];
        if (config == null || config.waveDatas == null || config.waveDatas.Count == 0)
        {
            OnWaveCompleted?.Invoke();
            return;
        }

        this.currentWaveIndex = waveIndex + 1;
        this.totalEnemiesInCurrentWave = 0;
        
        foreach (var data in config.waveDatas)
        {
            if (data != null)
                this.totalEnemiesInCurrentWave += data.GetTotalEnemyCount();
        }

        this.aliveEnemiesCount = this.totalEnemiesInCurrentWave;
        this.isWaveInProgress = true;
        
        NotifyUI();

        foreach (var coroutine in spawnCoroutines)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        spawnCoroutines.Clear();

        foreach (var data in config.waveDatas)
        {
            if (data != null)
            {
                spawnCoroutines.Add(StartCoroutine(SpawnRoutine(data)));
            }
        }
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