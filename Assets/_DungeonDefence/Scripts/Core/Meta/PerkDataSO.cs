using UnityEngine;
using System.Collections.Generic;

public enum PerkType
{
    StartGold,
    ShopSlot,
    CoreHealth,
    UnitAttackPower,
    ManaRegen
}

[CreateAssetMenu(fileName = "NewPerk", menuName = "DungeonDefence/Meta/PerkData")]
public class PerkDataSO : ScriptableObject
{
    public string perkId;
    public string perkName;
    [TextArea] public string description;
    public PerkType type;
    public Sprite icon;

    public int maxLevel = 5;
    public int baseCost = 100;
    public float costMultiplier = 1.5f;
    public float valuePerLevel = 10f;

    
    public Vector2 nodePosition;
    public List<string> prerequisitePerks;

    public int GetCost(int currentLevel)
    {
        return Mathf.FloorToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }

    public float GetTotalValue(int currentLevel)
    {
        return currentLevel * valuePerLevel;
    }
}



