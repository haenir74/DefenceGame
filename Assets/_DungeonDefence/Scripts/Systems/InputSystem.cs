using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem
{
    public Node RaycastNode(Camera camera, Vector3 mousePos, LayerMask layerMask)
    {
        Ray ray = camera.ScreenPointToRay(mousePos);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            return GridManager.Instance.GetNode(hit.point);
        }
        
        return null;
    }
}