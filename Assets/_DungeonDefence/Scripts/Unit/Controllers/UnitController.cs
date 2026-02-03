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

        Unit prefabComp = data.prefab.GetComponent<Unit>();
        if (prefabComp == null) return null;

        Unit unit = PoolManager.Instance.Spawn(prefabComp, node.WorldPosition, Quaternion.identity);

        if (unit != null)
        {
            unit.transform.SetParent(unitContainer);
            unit.Setup(data, node);
        }

        return unit;
    }
}
