using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask unitLayer;

    private Camera mainCamera;
    private GridNode lastHoveredNode;

    private void Awake()
    {
        this.mainCamera = Camera.main;
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (this.mainCamera == null) return;
        
        HandleMouseHover();
        HandleMouseClick();
    }

    private void HandleMouseHover()
    {
        GridNode currentNode = GetNodeUnderMouse();
        
        if (currentNode != lastHoveredNode)
        {
            InputManager.Instance?.TriggerHover(lastHoveredNode, currentNode);
            GridManager.Instance?.OnHoverChanged(lastHoveredNode, currentNode);
            lastHoveredNode = currentNode;
        }
    }

    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (TrySelectUnit()) return;
            GridNode node = GetNodeUnderMouse();
            if (node != null)
            {
                InputManager.Instance?.TriggerClick(node);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            GridNode node = GetNodeUnderMouse();
            if (node != null)
            {
                InputManager.Instance?.TriggerRightClick(node);
            }
        }
    }

    private bool TrySelectUnit()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayer))
        {
            Unit unit = hit.collider.GetComponent<Unit>();
            if (unit != null)
            {
                Debug.Log($"유닛 선택됨: {unit.name}");
                return true;
            }
        }
        return false;
    }

    private GridNode GetNodeUnderMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            return GridManager.Instance.GetNode(hit.point);
        }
        return null;
    }
}