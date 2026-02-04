using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Unit unit;
    private Vector3 targetWorldPos;

    public bool IsMoving { get; private set; }

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        IsMoving = false;

        transform.position = unit.transform.position; 
    }

    public void MoveTo(Vector2Int targetGridPos)
    {
        if (IsMoving) return;
        targetWorldPos = GridManager.Instance.GetWorldPosition(targetGridPos.x, targetGridPos.y);
        IsMoving = true;
    }

    public void OnUpdate()
    {
        if (!IsMoving) return;
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWorldPos) < 0.01f)
        {
            transform.position = targetWorldPos;
            IsMoving = false;

            if (unit != null)
            {
                GridNode node = GridManager.Instance.GetNode(transform.position);
                if (node != null)
                {
                    unit.OnReachTile(node.Coordinate);
                }
            }
        }
    }
}