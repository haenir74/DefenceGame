using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    [SerializeField] public LayerMask tileLayer; // Inspector에서 Tile 관련 레이어 할당
    [SerializeField] public LayerMask unitLayer; // Inspector에서 Unit 관련 레이어 할당

    private Camera mainCamera;
    private GridNode lastHoveredNode;

    private void Awake()
    {
        this.mainCamera = Camera.main;
    }

    private void Start()
    {
        InitializeLayerMasks();
    }

    private void InitializeLayerMasks()
    {
        // [FIX] 인스펙터에서 마스크가 설정되지 않았을 경우 GridManager의 데이터를 참조하여 자동 복구
        if (GridManager.Instance != null && GridManager.Instance.Data != null)
        {
            if (tileLayer.value == 0)
                tileLayer = GridManager.Instance.Data.groundLayer;

            if (unitLayer.value == 0)
                unitLayer = LayerMask.GetMask("Unit", "Allies", "Enemies");
        }
        else
        {
            // Fallback: 일반적인 기본값 설정
            if (tileLayer.value == 0) tileLayer = LayerMask.GetMask("Ground");
            if (unitLayer.value == 0) unitLayer = LayerMask.GetMask("Unit", "Allies", "Enemies");
        }
        Debug.Log($"[InputController] Layers Initialized - tileLayer: {tileLayer.value}, unitLayer: {unitLayer.value}");
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
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

            // [FIX] Priority Logic: Unit > Tile > Cancel

            // 1. Check for Units (Allies/Enemies/Units)
            Unit hitUnit = null;
            foreach (var hit in hits)
            {
                if (((1 << hit.collider.gameObject.layer) & unitLayer) != 0)
                {
                    hitUnit = hit.collider.GetComponent<Unit>() ?? hit.collider.GetComponentInParent<Unit>();
                    if (hitUnit != null) break;
                }
            }

            if (hitUnit != null)
            {
                Debug.Log($"[InputController] Unit clicked: {hitUnit.name}");
                InputManager.Instance?.TriggerUnitClick(hitUnit);
                return;
            }

            // 2. Check for Grid Nodes (Tiles) - using specific tileLayer
            GridNode hitNode = null;
            foreach (var hit in hits)
            {
                if (((1 << hit.collider.gameObject.layer) & tileLayer) != 0)
                {
                    var dropHandler = hit.collider.GetComponent<GridDropHandler>() ?? hit.collider.GetComponentInParent<GridDropHandler>();
                    if (dropHandler != null && dropHandler.TargetNode != null)
                    {
                        hitNode = dropHandler.TargetNode;
                        break;
                    }
                }
            }

            if (hitNode != null)
            {
                Debug.Log($"[InputController] Node detected and Triggered: {hitNode.Coordinate}");
                InputManager.Instance?.TriggerClick(hitNode);
                return;
            }

            // 3. Nothing hit -> Cancel Selection
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    Debug.Log($"[InputController] Hit: {hit.collider.name} | Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
                }
            }
            InputManager.Instance?.TriggerCancel();
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


    private GridNode GetNodeUnderMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        // 1순위: 유닛 노드 우선 확인
        foreach (var hit in hits)
        {
            if (((1 << hit.collider.gameObject.layer) & unitLayer) != 0)
            {
                Unit unit = hit.collider.GetComponent<Unit>() ?? hit.collider.GetComponentInParent<Unit>();
                if (unit != null && unit.CurrentNode != null)
                {
                    return unit.CurrentNode;
                }
            }
        }

        // 2순위: 타일 노드 확인
        foreach (var hit in hits)
        {
            if (((1 << hit.collider.gameObject.layer) & tileLayer) != 0)
            {
                var dropHandler = hit.collider.GetComponent<GridDropHandler>() ?? hit.collider.GetComponentInParent<GridDropHandler>();
                if (dropHandler != null && dropHandler.TargetNode != null)
                {
                    return dropHandler.TargetNode;
                }
            }
        }

        // 3순위: (혹시 모를) 레이어 마스크 제한 없는 GridDropHandler 검색
        foreach (var hit in hits)
        {
            var dropHandler = hit.collider.GetComponent<GridDropHandler>() ?? hit.collider.GetComponentInParent<GridDropHandler>();
            if (dropHandler != null && dropHandler.TargetNode != null)
            {
                return dropHandler.TargetNode;
            }
        }

        return null;
    }
}