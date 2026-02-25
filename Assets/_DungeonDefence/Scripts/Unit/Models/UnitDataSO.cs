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

[CreateAssetMenu(fileName = "New Unit Data", menuName = "DungeonDefence/Datas/Unit Data")]
public class UnitDataSO : ScriptableObject, IStorable, ITradable
{
    [Header("Basic Info")]
    public int unitIdNumber;
    public string unitId;
    public string unitName;
    [TextArea] public string description;

    [Header("Visuals")]
    public Sprite icon;
    public GameObject prefab;

    [Header("Type & Team")]
    public bool isPlayerTeam;
    public UnitCategory category;

    [Header("Combat Stats")]
    public float maxHp = 100f;
    public float maxMp = 100f;
    public float startMp = 0f;
    public float moveSpeed = 5f;
    public float basePower = 10f;
    public float attackInterval = 1f;

    [Header("Skill")]
    public SkillDataSO skill;

    [Header("Dispatch Stats")]
    [Tooltip("파견 시 자원 획득 효율 (1.0 = 100%)")]
    public float dispatchEfficiency = 1.0f;

    [Header("Economy (Player Unit)")]
    [SerializeField] private List<ResourceCost> costs;
    public List<ResourceCost> GetCosts() => costs;
    public int populationCost;
    [Tooltip("상점에서 구매 가능한 횟수. 0 = 무한")]
    public int shopStock = 5;

    // IStorable
    public int ID => unitIdNumber;
    public string Name => unitName;
    public string Description => description;
    public Sprite Icon => icon;
    public int MaxStack => 999;
}
