using UnityEngine;
using System;

public class WaveManager : Singleton<WaveManager>
{
    private int currentWave = 1;
    private int remainingEnemies;
    private int totalEnemies;
    
    // 현재웨이브, 남은적, 전체적
    public event Action<int, int, int> OnWaveInfoChanged;

    private void Start()
    {
        StartWave(1, 10); 
        
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnUnitDead += HandleUnitDead;
        }
    }

    public void StartWave(int waveIndex, int enemyCount)
    {
        this.currentWave = waveIndex;
        this.totalEnemies = enemyCount;
        this.remainingEnemies = enemyCount;

        NotifyUI();
    }

    private void HandleUnitDead(Unit unit)
    {
        if (unit.IsPlayerTeam) return;

        if (remainingEnemies > 0)
        {
            remainingEnemies--;
            NotifyUI();

            if (remainingEnemies <= 0)
            {
                Debug.Log($"[Wave] {currentWave} 웨이브 클리어!");
            }
        }
    }

    private void NotifyUI()
    {
        OnWaveInfoChanged?.Invoke(currentWave, remainingEnemies, totalEnemies);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (UnitManager.Instance != null)
            UnitManager.Instance.OnUnitDead -= HandleUnitDead;
    }
}