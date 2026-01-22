using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/Units/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("General")]
    public string unitName;
    public Team team;
    public GameObject prefab; // Visual Prefab (Optional if handled by manager)

    [Header("Base Stats")]
    public float maxHp = 100f;
    public float moveSpeed = 2f;

    [Header("Combat Stats")]
    public float attackRange = 3f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
}
