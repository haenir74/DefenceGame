using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory;

public enum UnitCategory
{
    None = 0,
    Normal,
    Tower,
    Core,
    Slime,
    Spider,
    Undead,
    Demon,
    Human
}

public enum UnitTier
{
    Basic = 0,        
    Intermediate = 1, 
    Advanced = 2,     
    Supreme = 3       
}

[CreateAssetMenu(fileName = "New Unit Data", menuName = "DungeonDefence/Datas/Unit Data")]
public class UnitDataSO : ScriptableObject, IStorable, ITradable
{
    
    public int unitIdNumber;
    public string unitId;
    public string unitName;
    [TextArea] public string description;

    
    public Sprite icon;
    public GameObject prefab;

    
    public bool isPlayerTeam;
    public UnitCategory category;
    
    public UnitTier tier;

    
    public float maxHp = 100f;
    public float maxMp = 100f;
    public float startMp = 0f;
    public float moveSpeed = 5f;
    public float basePower = 10f;
    public float attackInterval = 1f;

    
    public SkillDataSO skill;

    
    
    public float dispatchEfficiency = 1.0f;

    
    [SerializeField] private List<ResourceCost> costs;
    public List<ResourceCost> GetCosts() => costs;
    public int populationCost;
    
    public int shopStock = 5;

    
    public int ID => unitIdNumber;
    public string Name => unitName;
    public string Description => description;
    public Sprite Icon => icon;
    public int MaxStack => 999;
}



