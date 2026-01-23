using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/Units/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("General")]
    public string unitName;
    public Team team;
    public int cost = 30;
    public bool isCore;
    public GameObject prefab;

    [Header("Base Stats")]
    public float maxHp = 100f;
    public int mp = 0;
    public float moveSpeed = 2f;

    [Header("Combat Stats")]
    public float attackDamage = 10f;
    public float attackSpeed = 1f;
    public float critRate = 0f;
    public float critDamage = 50f;
    public float armor = 0f;
}
