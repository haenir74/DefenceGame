using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private LayerMask groundLayer;

    private Tile lastHoveredTile;
    
    void Update()
    {
        HandleMouseHover();
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    void HandleMouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out var hit, 100f, groundLayer)) {
            ResetHover();
            return;
        }

        Node node = gridManager.GetNode(hit.point);
        if (node == null) {
            ResetHover();
            return;
        }

        Tile currentTile = node.CurrentTile;
        if (currentTile == lastHoveredTile) return;

        lastHoveredTile?.OnHoverExit();
        currentTile?.OnHoverEnter();
        lastHoveredTile = currentTile;
    }

    void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Node node = gridManager.GetNode(hit.point);
            if (node != null)
            {
                Debug.Log("Clicked on Node at: " + node.X + ", " + node.Y);
                node.CurrentTile?.OnStepped();
            }
        }
    }

    void ResetHover()
    {
        lastHoveredTile?.OnHoverExit();
        lastHoveredTile = null;
    }
}