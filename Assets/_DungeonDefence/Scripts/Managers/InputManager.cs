using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private LayerMask groundLayer;

    private InputSystem inputSystem;
    private Camera mainCamera;
    private Node lastHoveredNode;

    public event Action<Node> OnClickNode;
    public event Action<Node> OnRightClickNode;
    public event Action<Node, Node> OnHoverNodeChanged;

    void Awake()
    {
        Instance = this;
        inputSystem = new InputSystem();
        mainCamera = Camera.main;
    }

    void Update()
    {
        Node currentNode = inputSystem.RaycastNode(mainCamera, Input.mousePosition, groundLayer);

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
        
        if (Input.GetMouseButtonDown(1)) // Right Click
        {
            if (currentNode != null)
            {
                OnRightClickNode?.Invoke(currentNode);
            }
        }
    }
}
