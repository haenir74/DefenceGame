using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputManager : Singleton<InputManager>
{
    [Header("Settings")]
    [SerializeField] private LayerMask groundLayer;

    private InputSystem inputSystem;
    private Camera mainCamera;
    private Node lastHoveredNode;

    public event Action<Node> OnClickNode;
    public event Action<Node> OnRightClickNode;
    public event Action<Node, Node> OnHoverNodeChanged;

    protected override void Awake()
    {
        base.Awake();
        inputSystem = new InputSystem();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Vector3? hitPoint = inputSystem.GetMouseWorldPosition(mainCamera, Input.mousePosition, groundLayer);
        Node currentNode = null;

        if (hitPoint.HasValue && GridManager.Instance != null)
        {
            currentNode = GridManager.Instance.GetNode(hitPoint.Value);
        }

        if (currentNode != lastHoveredNode)
        {
            OnHoverNodeChanged?.Invoke(lastHoveredNode, currentNode);
            lastHoveredNode = currentNode;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (currentNode != null)
            {
                OnClickNode?.Invoke(currentNode);
            }
        }
        
        if (Input.GetMouseButtonDown(1)) 
        {
            if (currentNode != null)
            {
                OnRightClickNode?.Invoke(currentNode);
            }
        }
    }
}
