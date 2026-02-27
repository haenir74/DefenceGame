using UnityEngine;
using System.Collections.Generic;

public enum PerkType
{
    CoreHealthIncrease,
    StartingGoldIncrease,
    ShopSlotUnlock,
    AllyHealthIncrease,
    AllyAttackIncrease,
    StartingUnitAddition
}

[CreateAssetMenu(fileName = "NewPerk", menuName = "DungeonDefence/Meta/PerkData")]
public class PerkDataSO : ScriptableObject
{
    public string PerkID;
    public string DisplayName;
    [TextArea] public string Description;
    public int Cost;
    public PerkDataSO[] Prerequisites;
    public PerkType Type;
    public float NumericValue;
    public string StringValue;
    public Sprite icon;

    public Vector2 nodePosition;
}




