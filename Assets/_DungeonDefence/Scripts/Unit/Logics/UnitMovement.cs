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

        GridNode targetNode = GridManager.Instance.GetNode(targetGridPos.x, targetGridPos.y);
        if (targetNode == null) return;

        // 빈 슬롯 위치를 목적지로 사용. 슬롯이 없으면 타일 중앙으로 이동.
        float cellSize = GridManager.Instance?.Data?.cellSize ?? 1f;
        Vector3? slotPos = targetNode.TryOccupySlot(unit, cellSize);
        targetWorldPos = slotPos ?? targetNode.WorldPosition;
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
