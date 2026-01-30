using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Data", menuName = "DungeonDefence/Unit/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string unitName;
    public GameObject prefab;
    public Sprite icon;

    [Header("Stats")]
    public float maxHp = 100f;
    public float moveSpeed = 5f;

    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackRange = 1f;
    public float attackInterval = 1.0f;

    [Header("Team")]
    public bool isPlayerTeam;
}
