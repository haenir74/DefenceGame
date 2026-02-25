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
    private int pendingSpawnCount;  // 아직 스폰되지 않은 적 수
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
        this.pendingSpawnCount = this.totalEnemiesInCurrentWave; // 전부 미스폰 상태로 시작
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
                pendingSpawnCount--; // 스폰 완료 1건
                yield return new WaitForSeconds(group.spawnInterval);
            }
        }

        // 이 루틴 종료 후 웨이브 완료 여부 체크
        CheckWaveFinished();
    }

    private void HandleUnitDead(Unit unit)
    {
        if (!isWaveInProgress || unit.IsPlayerTeam) return;

        aliveEnemiesCount--;
        if (aliveEnemiesCount < 0) aliveEnemiesCount = 0;

        NotifyUI();

        // 아직 스폰 중이면 종료 판정 보류
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
        Debug.Log($"[WaveManager] 웨이브 {currentWaveIndex} 클리어!");

        if (UnitManager.Instance != null)
            UnitManager.Instance.NotifyWaveClear();

        OnWaveCompleted?.Invoke();
    }

    /// <summary>다음 웨이브의 티어 확률 반환 (상점 롤에 사용)</summary>
    public TierProbabilities GetNextWaveTierProbs()
    {
        int nextIndex = currentWaveIndex; // 클리어 후이므로 currentWaveIndex == 다음 웨이브 인덱스
        if (waves == null || nextIndex < 0 || nextIndex >= waves.Count) return null;
        WaveConfig config = waves[nextIndex];
        if (config == null || config.waveDatas == null || config.waveDatas.Count == 0) return null;
        // 첫 번째 WaveData의 확률을 대표값으로 사용
        return config.waveDatas[0]?.shopTierWeights;
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