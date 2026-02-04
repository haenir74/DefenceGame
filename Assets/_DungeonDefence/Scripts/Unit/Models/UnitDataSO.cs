using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitCategory
{
    Normal,
    Tower,
    Core
}

[CreateAssetMenu(fileName = "New Unit Data", menuName = "DungeonDefence/Unit/Unit Data")]
public class UnitDataSO : ScriptableObject//, ITradable
{
    [Header("Basic Info")]
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
    public int cost;
    public int populationCost;

    // ITradable
    public string Name => unitName;
    public int Cost => cost;
    public Sprite Icon => icon;
}
