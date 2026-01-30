using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    private Unit _unit;
    private float _moveSpeed = 5f;

    public void Setup(Unit unit)
    {
        _unit = unit;
    }

    public void MoveTo(GridNode targetNode, System.Action onComplete = null)
    {
        if (targetNode == null) return;
        StartCoroutine(MoveRoutine(targetNode, onComplete));
    }

    private IEnumerator MoveRoutine(GridNode targetNode, System.Action onComplete)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = targetNode.WorldPosition; 
        
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * _moveSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
        _unit.SetNode(targetNode);
        
        onComplete?.Invoke();
    }
}