using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputLogic
{
    public Vector3? GetMouseWorldPosition(Camera camera, Vector3 mousePos, LayerMask layerMask)
    {
        if (camera == null) return null;
        Ray ray = camera.ScreenPointToRay(mousePos);
        return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask) ? hit.point : (Vector3?)null;
    }
}