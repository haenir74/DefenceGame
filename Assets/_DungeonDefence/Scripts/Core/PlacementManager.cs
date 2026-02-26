using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementManager : Singleton<PlacementManager>
{
    public bool IsInPlacementMode => DragDropManager.Instance.IsDragging || IsItemSelected;
    public bool IsItemSelected => GameManager.Instance.SelectedUnitToPlace != null || GameManager.Instance.SelectedTileToPlace != null;

    
    
    
    public void ExecutePlacement(DragPayload payload, GameObject target = null)
    {
        if (payload == null) return;

        
        GridNode targetNode = target?.GetComponent<GridDropHandler>()?.TargetNode;

        
        DispatchDropHandler dispatchHandler = target?.GetComponentInParent<DispatchDropHandler>();
        DispatchSlotUI targetSlot = dispatchHandler?.GetComponent<DispatchSlotUI>();

        bool isInventory = target?.GetComponentInParent<InventoryDropHandler>() != null;

        

        bool success = false;

        
        if (targetNode != null)
        {
            success = TryPlaceOnGrid(payload, targetNode);
        }
        
        else if (dispatchHandler != null)
        {
            success = TryPlaceInDispatch(payload, targetSlot);
        }
        
        else if (isInventory)
        {
            success = RecallToInventory(payload);
        }
        
        else
        {
            
            success = RecallToInventory(payload);
        }

        
        if (success)
        {
            
            CleanupSource(payload);
        }
        else
        {
            
            CancelPlacement(payload);
        }

        
        DragDropManager.Instance.EndDrag(true);
        GameManager.Instance.ClearSelection(false);
    }

    
    
    
    public void CancelPlacement(DragPayload payload)
    {
        if (payload == null) return;

        

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
        
        if (payload.UnitData != null || (payload.GridUnit != null))
        {
            UnitDataSO data = payload.UnitData ?? payload.GridUnit.Data;

            if (!node.CanPlaceUnit)
            {
                
                return false;
            }

            
            if (payload.Source == DragPayload.SourceType.Inventory)
            {
                if (!InventoryManager.Instance.TryConsumeItem(data)) return false;
            }
            

            UnitManager.Instance.SpawnUnit(data, node);
            return true;
        }
        
        else if (payload.TileData != null)
        {
            if (node == GridManager.Instance.GetCoreNode() || node == GridManager.Instance.GetSpawnNode()) return false;
            if (node.CurrentTileData != null && node.CurrentTileData.ID == payload.TileData.ID) return false;

            if (InventoryManager.Instance.TryConsumeItem(payload.TileData))
            {
                
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

        
        if (slot == null)
        {
            if (DispatchPanelUI.Instance != null)
            {
                
                if (payload.Source == DragPayload.SourceType.Inventory)
                {
                    if (!InventoryManager.Instance.TryConsumeItem(data)) return false;
                }

                var newSlot = DispatchPanelUI.Instance.CreateSlotAndAssign(payload);
                return newSlot != null;
            }
            return false;
        }

        
        if (payload.Source == DragPayload.SourceType.Inventory)
        {
            if (InventoryManager.Instance.TryConsumeItem(data))
            {
                slot.AssignUnitData(data);
                return true;
            }
            return false;
        }

        
        slot.AssignUnitData(data);
        return true;
    }

    private bool RecallToInventory(DragPayload payload)
    {
        if (payload.UnitData != null)
        {
            
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
            payload.FromSlot.ClearSlot(false); 
        }
    }

    
    
    
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
                
                return mainCam.WorldToScreenPoint(node.WorldPosition);
            }
        }
        return screenPos;
    }
}



