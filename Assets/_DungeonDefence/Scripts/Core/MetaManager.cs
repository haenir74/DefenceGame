using UnityEngine;
using System.Collections.Generic;

public class MetaManager : Singleton<MetaManager>
{
    private const string PERK_POINTS_KEY = "Meta_PerkPoints";
    private const string PERK_UPGRADE_PREFIX = "Meta_Perk_";

    [SerializeField] private List<PerkDataSO> availablePerks;

    public struct RunResult
    {
        public bool isVictory;
        public int waves;
        public int kills;
        public int gold;
        public int score;
        public int earnedPoints;
    }

    public RunResult LastRun { get; private set; }
    public int PerkPoints { get; private set; }
    public IReadOnlyList<PerkDataSO> AvailablePerks => availablePerks;
    private Dictionary<string, int> perkLevels = new Dictionary<string, int>();

    protected override bool DontDestroy => true;

    protected override void Awake()
    {
        base.Awake();
        LoadData();
    }

    public void AddPerkPoints(int amount)
    {
        PerkPoints += amount;
        SaveData();
    }

    public bool TrySpendPerkPoints(int amount)
    {
        if (PerkPoints >= amount)
        {
            PerkPoints -= amount;
            SaveData();
            return true;
        }
        return false;
    }

    public int GetPerkLevel(string perkId)
    {
        return perkLevels.ContainsKey(perkId) ? perkLevels[perkId] : 0;
    }

    public void SetRunResult(bool isVictory, int waves, int kills, int gold)
    {
        int score = ScoreEvaluator.CalculateScore(waves, kills, gold);
        int points = ScoreEvaluator.CalculatePerkPoints(score);

        LastRun = new RunResult
        {
            isVictory = isVictory,
            waves = waves,
            kills = kills,
            gold = gold,
            score = score,
            earnedPoints = points
        };

        AddPerkPoints(points);
    }

    public void UpgradePerk(string perkId)
    {
        PerkDataSO perk = availablePerks.Find(p => p.perkId == perkId);
        if (perk == null) return;

        int currentLevel = GetPerkLevel(perkId);
        if (currentLevel >= perk.maxLevel) return;

        int cost = perk.GetCost(currentLevel);
        if (TrySpendPerkPoints(cost))
        {
            if (perkLevels.ContainsKey(perkId))
                perkLevels[perkId]++;
            else
                perkLevels[perkId] = 1;
            SaveData();
        }
    }

    private void LoadData()
    {
        PerkPoints = PlayerPrefs.GetInt(PERK_POINTS_KEY, 0);
        perkLevels.Clear();
        if (availablePerks != null)
        {
            foreach (var perk in availablePerks)
            {
                int level = PlayerPrefs.GetInt(PERK_UPGRADE_PREFIX + perk.perkId, 0);
                perkLevels[perk.perkId] = level;
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PERK_POINTS_KEY, PerkPoints);
        foreach (var kvp in perkLevels)
        {
            PlayerPrefs.SetInt(PERK_UPGRADE_PREFIX + kvp.Key, kvp.Value);
        }
        PlayerPrefs.Save();
    }
}



