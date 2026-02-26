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
    
    [SerializeField] private List<WaveConfig> waves;

    public event Action<int, int, int> OnWaveInfoChanged;
    public event Action OnWaveCompleted;

    private int currentWaveIndex = 0;
    private int totalEnemiesInCurrentWave;
    private int aliveEnemiesCount;
    private int pendingSpawnCount;
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
        this.pendingSpawnCount = this.totalEnemiesInCurrentWave;
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
                pendingSpawnCount--;
                yield return new WaitForSeconds(group.spawnInterval);
            }
        }

        CheckWaveFinished();
    }

    private void HandleUnitDead(Unit unit)
    {
        if (!isWaveInProgress || unit.IsPlayerTeam) return;

        aliveEnemiesCount--;
        if (aliveEnemiesCount < 0) aliveEnemiesCount = 0;

        NotifyUI();

        if (pendingSpawnCount > 0) return;

        CheckWaveFinished();
    }

    private void CheckWaveFinished()
    {
        if (!isWaveInProgress) return;
        if (pendingSpawnCount > 0) return;
        if (aliveEnemiesCount > 0) return;

        FinishWave();
    }

    private void FinishWave()
    {
        isWaveInProgress = false;
        

        if (UnitManager.Instance != null)
            UnitManager.Instance.NotifyWaveClear();

        OnWaveCompleted?.Invoke();
    }

    public TierProbabilities GetNextWaveTierProbs()
    {
        TierProbabilities probs = new TierProbabilities();
        int nextWave = currentWaveIndex + 1;

        if (nextWave < 10)
        {
            probs.basicWeight = 70;
            probs.intermediateWeight = 30;
            probs.advancedWeight = 0;
            probs.supremeWeight = 0;
        }
        else if (nextWave < 20)
        {
            probs.basicWeight = 40;
            probs.intermediateWeight = 30;
            probs.advancedWeight = 30;
            probs.supremeWeight = 0;
        }
        else
        {
            probs.basicWeight = 20;
            probs.intermediateWeight = 40;
            probs.advancedWeight = 30;
            probs.supremeWeight = 10;
        }

        return probs;
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


