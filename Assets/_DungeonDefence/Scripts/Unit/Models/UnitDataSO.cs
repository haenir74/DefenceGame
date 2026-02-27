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

[System.Flags]
public enum UnitTag
{
    None        = 0,
    Melee       = 1 << 0,
    Ranged      = 1 << 1,
    Magic       = 1 << 2,
    Monster     = 1 << 3,
    Slime       = 1 << 4,
    Spider      = 1 << 5,
    Undead      = 1 << 6,
    Demon       = 1 << 7,
    Human       = 1 << 8
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

    [Header("Meta Categories")]
    public UnitTag tags;

    [Header("Combat Statistics")]
    [Tooltip("Demon 빌드의 경우, 복잡한 메커니즘 없이 basePower, maxHp, baseDefense를 높게 설정하세요.")]
    public float maxHp = 100f;
    public float maxMp = 100f;
    public float startMp = 0f;
    public float moveSpeed = 5f;
    
    public float basePower = 10f;
    public float baseDefense = 0f;
    public float attackRange = 1f;
    public float attackInterval = 1f;

    public bool HasTag(UnitTag tag) => (tags & tag) == tag;

    
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



