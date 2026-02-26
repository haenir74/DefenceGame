using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementManager : Singleton<PlacementManager>
{
    public bool IsInPlacementMode => DragDropManager.Instance.IsDragging || IsItemSelected;
    public bool IsItemSelected => GameManager.Instance.SelectedUnitToPlace != null || GameManager.Instance.SelectedTileToPlace != null;

    /// <summary>
    /// 메인 진입점: 드롭이 발생했을 때 호출됩니다.
    /// </summary>
    public void ExecutePlacement(DragPayload payload, GameObject target = null)
    {
        if (payload == null) return;

        // 타겟 식별
        GridNode targetNode = target?.GetComponent<GridDropHandler>()?.TargetNode;

        // [FIX] Dispatch target detection: Check for DispatchDropHandler on the target or its hierarchy
        DispatchDropHandler dispatchHandler = target?.GetComponentInParent<DispatchDropHandler>();
        DispatchSlotUI targetSlot = dispatchHandler?.GetComponent<DispatchSlotUI>();

        bool isInventory = target?.GetComponentInParent<InventoryDropHandler>() != null;

        Debug.Log($"[PlacementManager] Executing: Source={payload.Source} -> TargetNode={targetNode?.Coordinate} HasDispatchHandler={dispatchHandler != null} IsInventory={isInventory}");

        bool success = false;

        // 1. Grid 배치
        if (targetNode != null)
        {
            success = TryPlaceOnGrid(payload, targetNode);
        }
        // 2. Dispatch 배치
        else if (dispatchHandler != null)
        {
            success = TryPlaceInDispatch(payload, targetSlot);
        }
        // 3. Inventory 회수
        else if (isInventory)
        {
            success = RecallToInventory(payload);
        }
        // 4. Fallback: 아무것도 아닌 곳에 놓았을 때 인벤토리로 회수
        else
        {
            Debug.Log("[PlacementManager] No valid target. Falling back to inventory recall.");
            success = RecallToInventory(payload);
        }

        // 결과에 따른 처리
        if (success)
        {
            // 드래그 종료 시 원본 소스에서 제거 (이미 수행된 경우 제외)
            CleanupSource(payload);
        }
        else
        {
            // 실패 시 원본 상태 복구 (드래그 취소와 동일한 처리)
            CancelPlacement(payload);
        }

        // 최종적으로 선택 해제 및 드래그 종료
        DragDropManager.Instance.EndDrag(true);
        GameManager.Instance.ClearSelection(false);
    }

    /// <summary>
    /// 드래그 취소 시 숨겨졌던 원본 항목들을 다시 표시하거나 복구합니다.
    /// </summary>
    public void CancelPlacement(DragPayload payload)
    {
        if (payload == null) return;

        Debug.Log($"[Placement] Canceling drag from {payload.Source}. Restoring source.");

        if (payload.Source == DragPayload.SourceType.Grid && payload.GridUnit != null)
        {
            payload.GridUnit.SetVisualVisible(true);
        }
        else if (payload.Source == DragPayload.SourceType.Dispatch && payload.FromSlot != null)
        {
            payload.FromSlot.RestoreSlot();
        }

        GameManager.Instance.ClearSelection(false);
    }

    private bool TryPlaceOnGrid(DragPayload payload, GridNode node)
    {
        // 유닛 배치
        if (payload.UnitData != null || (payload.GridUnit != null))
        {
            UnitDataSO data = payload.UnitData ?? payload.GridUnit.Data;

            if (!node.CanPlaceUnit)
            {
                Debug.LogWarning($"[Placement] Cannot place unit on node {node.Coordinate}");
                return false;
            }

            // 소스에 따른 아이템 소모/이동 처리
            if (payload.Source == DragPayload.SourceType.Inventory)
            {
                if (!InventoryManager.Instance.TryConsumeItem(data)) return false;
            }
            // Grid -> Grid 이동이나 Dispatch -> Grid 이동은 나중에 CleanupSource에서 처리됨 (Despawn 등)

            UnitManager.Instance.SpawnUnit(data, node);
            return true;
        }
        // 타일 배치
        else if (payload.TileData != null)
        {
            if (node == GridManager.Instance.GetCoreNode() || node == GridManager.Instance.GetSpawnNode()) return false;
            if (node.CurrentTileData != null && node.CurrentTileData.ID == payload.TileData.ID) return false;

            if (InventoryManager.Instance.TryConsumeItem(payload.TileData))
            {
                // 이전 타일 회수
                if (node.CurrentTileData != null && !node.CurrentTileData.IsDefaultTile)
                {
                    InventoryManager.Instance.AddItem(node.CurrentTileData, 1);
                }
                GridManager.Instance.ChangeTile(node, payload.TileData);
                return true;
            }
        }
        return false;
    }

    private bool TryPlaceInDispatch(DragPayload payload, DispatchSlotUI slot)
    {
        if (payload.UnitData == null && payload.GridUnit == null) return false;
        UnitDataSO data = payload.UnitData ?? payload.GridUnit?.Data;

        // [FIX] Drop on background (slot == null) -> Request panel to create a new slot
        if (slot == null)
        {
            if (DispatchPanelUI.Instance != null)
            {
                // 소모 로직은 CreateSlotAndAssign 내부가 아닌 여기서 통합 관리 (트랜잭션)
                if (payload.Source == DragPayload.SourceType.Inventory)
                {
                    if (!InventoryManager.Instance.TryConsumeItem(data)) return false;
                }

                var newSlot = DispatchPanelUI.Instance.CreateSlotAndAssign(payload);
                return newSlot != null;
            }
            return false;
        }

        // Drop on a specific existing slot
        if (payload.Source == DragPayload.SourceType.Inventory)
        {
            if (InventoryManager.Instance.TryConsumeItem(data))
            {
                slot.AssignUnitData(data);
                return true;
            }
            return false;
        }

        // From Grid/Dispatch
        slot.AssignUnitData(data);
        return true;
    }

    private bool RecallToInventory(DragPayload payload)
    {
        if (payload.UnitData != null)
        {
            // Inventory -> Inventory는 아무것도 하지 않음
            if (payload.Source == DragPayload.SourceType.Inventory) return true;

            InventoryManager.Instance.AddItem(payload.UnitData, 1);
            return true;
        }
        else if (payload.GridUnit != null)
        {
            InventoryManager.Instance.AddItem(payload.GridUnit.Data, 1);
            return true;
        }
        else if (payload.TileData != null)
        {
            // 타일은 이미 인벤토리에 있거나(드래그 중 소모 안 되었을 때) 소모 전 상태
            return true;
        }
        return false;
    }

    private void CleanupSource(DragPayload payload)
    {
        if (payload.Source == DragPayload.SourceType.Grid && payload.GridUnit != null)
        {
            UnitManager.Instance.DespawnUnit(payload.GridUnit);
        }
        else if (payload.Source == DragPayload.SourceType.Dispatch && payload.FromSlot != null)
        {
            payload.FromSlot.ClearSlot(false); // [FIX] No return to inventory as it was successfully placed
        }
    }

    /// <summary>
    /// 고스트 이미지의 위치를 결정합니다. 그리드 위라면 스내핑 처리.
    /// </summary>
    public Vector3 GetGhostPosition(Vector2 screenPos)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return screenPos;

        Ray ray = mainCam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            GridNode node = GridManager.Instance.GetNode(hit.point);
            if (node != null)
            {
                // 월드 좌표를 화면 좌표로 변환하여 반환 (스내핑 효과)
                return mainCam.WorldToScreenPoint(node.WorldPosition);
            }
        }
        return screenPos;
    }
}
