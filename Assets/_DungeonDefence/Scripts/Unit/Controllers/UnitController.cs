using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform unitContainer;

    public Unit SpawnUnit(UnitDataSO data, GridNode node)
    {
        if (data == null || data.prefab == null || node == null) return null;

        Vector3 spawnPos = node.WorldPosition; 
        
        GameObject unitObj = Instantiate(data.prefab, spawnPos, Quaternion.identity, unitContainer);
        Unit unit = unitObj.GetComponent<Unit>();

        if (unit != null)
        {
            unit.Setup(data, node);
        }

        return unit;
    }
}
