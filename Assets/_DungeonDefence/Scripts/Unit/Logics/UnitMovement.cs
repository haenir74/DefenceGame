using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Unit unit;
    private Vector3 targetWorldPos;
    private GridNode targetNode;  // 이동 중인 목적지 노드 (슬롯 해제용)

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

        // ★ 월드 Y는 유닛의 현재 높이를 그대로 유지 (X·Z만 이동)
        targetWorldPos = new Vector3(rawTarget.x, transform.position.y, rawTarget.z);
        targetNode = node;
        IsMoving = true;
    }

    /// <summary>이동 중 사망 등 비정상 중단 시 목적지 슬롯을 해제.</summary>
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
                // ★ GetNode는 X·Z만 사용 (Y 무시) — GridSystem.GetNode(worldPos) 구현 확인됨
                GridNode node = GridManager.Instance.GetNode(transform.position);
                if (node != null)
                {
                    unit.OnReachTile(node.Coordinate);
                }
            }
        }
    }
}
