using UnityEngine;
using System.Collections.Generic;

public class MetaManager : Singleton<MetaManager>
{
    private const string PERK_POINTS_KEY = "Meta_PerkPoints";
    private const string PERK_UPGRADE_PREFIX = "Meta_Perk_";

    [SerializeField] private List<PerkDataSO> availablePerks = new List<PerkDataSO>();

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
    }

    public void Initialize()
    {
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

    public bool IsPerkUnlocked(string perkId)
    {
        return perkLevels.ContainsKey(perkId) && perkLevels[perkId] > 0;
    }

    public float GetPerkValue(string perkId)
    {
        if (!IsPerkUnlocked(perkId)) return 0f;
        var perk = availablePerks?.Find(p => p.PerkID == perkId);
        return perk != null ? perk.NumericValue : 0f;
    }

    private float GetTotalNumericValueForType(PerkType type)
    {
        if (availablePerks == null) return 0f;
        float total = 0f;
        foreach (var perk in availablePerks)
        {
            if (perk != null && perk.Type == type && IsPerkUnlocked(perk.PerkID))
            {
                total += perk.NumericValue;
            }
        }
        return total;
    }

    public int GetTotalBonusStartingGold() => Mathf.FloorToInt(GetTotalNumericValueForType(PerkType.StartingGoldIncrease));
    public int GetTotalBonusCoreHealth() => Mathf.FloorToInt(GetTotalNumericValueForType(PerkType.CoreHealthIncrease));
    public int GetTotalUnlockedShopSlots() => Mathf.FloorToInt(GetTotalNumericValueForType(PerkType.ShopSlotUnlock));
    public float GetTotalBonusAllyHealth() => GetTotalNumericValueForType(PerkType.AllyHealthIncrease);
    public float GetTotalBonusAllyAttack() => GetTotalNumericValueForType(PerkType.AllyAttackIncrease);

    public List<string> GetStartingUnits()
    {
        List<string> units = new List<string>();
        if (availablePerks == null) return units;

        foreach (var perk in availablePerks)
        {
            if (perk != null && perk.Type == PerkType.StartingUnitAddition && IsPerkUnlocked(perk.PerkID))
            {
                if (!string.IsNullOrEmpty(perk.StringValue))
                {
                    units.Add(perk.StringValue);
                }
            }
        }
        return units;
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

    public void ResetRunResult()
    {
        LastRun = new RunResult();
    }

    public bool TryUnlockPerk(PerkDataSO perk)
    {
        if (perk == null) return false;

        // 이미 해금된 경우
        if (IsPerkUnlocked(perk.PerkID)) return false;

        // 선행 특전 조건 확인
        if (perk.Prerequisites != null)
        {
            foreach (var pre in perk.Prerequisites)
            {
                if (pre != null && !IsPerkUnlocked(pre.PerkID))
                {
                    Debug.Log($"Prerequisite {pre.DisplayName} not met for {perk.DisplayName}");
                    return false;
                }
            }
        }

        // 재화 확인 및 소모
        if (TrySpendPerkPoints(perk.Cost))
        {
            perkLevels[perk.PerkID] = 1;
            SaveData();
            return true;
        }

        return false;
    }

    private void LoadData()
    {
        PerkPoints = PlayerPrefs.GetInt(PERK_POINTS_KEY, 0);
        perkLevels.Clear();
        if (availablePerks != null)
        {
            foreach (var perk in availablePerks)
            {
                int unlocked = PlayerPrefs.GetInt(PERK_UPGRADE_PREFIX + perk.PerkID, 0);
                perkLevels[perk.PerkID] = unlocked;
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



