using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputController : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;

    private InputLogic inputLogic;
    private Camera mainCamera;
    private GridNode lastHoveredNode;

    private void Awake()
    {
        inputLogic = new InputLogic();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Vector3? hitPoint = inputLogic.GetMouseWorldPosition(mainCamera, Input.mousePosition, groundLayer);
        GridNode currentNode = null;

        if (hitPoint.HasValue && GridManager.Instance != null)
        {
            currentNode = GridManager.Instance.GetNode(hitPoint.Value);
        }

        // 호버 이벤트 처리
        if (currentNode != lastHoveredNode)
        {
            InputManager.Instance.TriggerHover(lastHoveredNode, currentNode);
            GridManager.Instance.OnHoverChanged(lastHoveredNode, currentNode);
            
            lastHoveredNode = currentNode;
        }

        // 클릭 이벤트 처리
        if (Input.GetMouseButtonDown(0) && currentNode != null)
        {
            InputManager.Instance.TriggerClick(currentNode);
        }
        
        // 우클릭 이벤트 처리
        if (Input.GetMouseButtonDown(1) && currentNode != null)
        {
            InputManager.Instance.TriggerRightClick(currentNode);
        }
    }
}
