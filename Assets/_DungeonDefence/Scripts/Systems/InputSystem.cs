using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem
{
    public Vector3? GetMouseWorldPosition(Camera camera, Vector3 mousePos, LayerMask layerMask)
    {
        if (camera == null) return null;
        
        Ray ray = camera.ScreenPointToRay(mousePos);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            return hit.point;
        }
        
        return null;
    }
}