using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Unit unit;
    private Vector3 targetWorldPos;
    private GridNode targetNode;  

    public bool IsMoving { get; private set; }
    public float SpeedMultiplier { get; set; } = 1.0f;
    public bool IsRooted { get; set; } = false;

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        this.targetNode = null;
        IsMoving = false;
        SpeedMultiplier = 1.0f;
        IsRooted = false;
    }

    public void MoveTo(Vector2Int targetGridPos)
    {
        if (IsMoving) return;

        GridNode node = GridManager.Instance.GetNode(targetGridPos.x, targetGridPos.y);
        if (node == null) return;

        float cellSize = GridManager.Instance?.Data?.cellSize ?? 1f;
        Vector3? slotPos = node.TryOccupySlot(unit, cellSize);
        Vector3 rawTarget = slotPos ?? node.WorldPosition;

        
        targetWorldPos = new Vector3(rawTarget.x, transform.position.y, rawTarget.z);
        targetNode = node;
        IsMoving = true;
    }

    
    public void CancelMove()
    {
        if (IsMoving && targetNode != null)
        {
            targetNode.ReleaseSlot(unit);
        }
        targetNode = null;
        IsMoving = false;
    }

    public void OnUpdate()
    {
        if (!IsMoving || IsRooted) return;

        float currentSpeed = moveSpeed * SpeedMultiplier;
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWorldPos) < 0.01f)
        {
            transform.position = targetWorldPos;
            IsMoving = false;
            targetNode = null;

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



