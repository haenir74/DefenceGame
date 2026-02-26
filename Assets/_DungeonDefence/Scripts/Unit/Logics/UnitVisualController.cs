using UnityEngine;

public class UnitVisualController : MonoBehaviour
{
    private Transform visualRoot;
    private bool isReady = false;

    
    public void Apply()
    {
        visualRoot = FindVisualRoot();
        if (visualRoot == null)
        {
            
            return;
        }

        
        visualRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);

        
        Vector3 pos = visualRoot.localPosition;
        pos.y = 0f;
        visualRoot.localPosition = pos;

        isReady = true;
    }

    
    
    
    
    private void LateUpdate()
    {
        if (!isReady || visualRoot == null) return;
        visualRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private Transform FindVisualRoot()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponentInChildren<SpriteRenderer>(true) != null)
                return child;
        }
        return null;
    }
}



