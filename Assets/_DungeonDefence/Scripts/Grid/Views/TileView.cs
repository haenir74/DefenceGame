using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileView : MonoBehaviour
{
    public GridNode Node { get; private set; }
    public TileDataSO Data { get; private set; }

    private GameObject currentVisualObject;
    private GameObject defaultPrefab;

    public void SetDefaultPrefab(GameObject prefab)
    {
        this.defaultPrefab = prefab;
    }

    public void Setup(GridNode node, TileDataSO data)
    {
        this.Node = node;
        
        if (this.Node != null)
        {
            this.Node.CurrentTile = this;
            gameObject.name = $"Cell_{node.X}_{node.Y}";
        }

        UpdateVisual(data);
    }

    public void UpdateVisual(TileDataSO newData)
    {
        this.Data = newData;
        if (this.Data == null) return;
        if (currentVisualObject != null)
        {
            Destroy(currentVisualObject);
            currentVisualObject = null;
        }
        GameObject prefabToSpawn = this.Data.prefab;
        if (prefabToSpawn == null)
        {
            prefabToSpawn = this.defaultPrefab;
        }
        if (prefabToSpawn != null)
        {
            currentVisualObject = Instantiate(prefabToSpawn, transform);
            currentVisualObject.transform.localPosition = Vector3.zero;
            currentVisualObject.transform.localRotation = Quaternion.identity;
            
            Renderer rend = currentVisualObject.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                if (this.Data.tileSprite != null)
                {
                    rend.material.mainTexture = this.Data.tileSprite.texture;
                }
            }
        }
    }

    public void SetHighlight(bool isActive)
    {
        if (currentVisualObject == null) return;

        var renderers = currentVisualObject.GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            if (rend.material.HasProperty("_Color"))
            {
                rend.material.color = isActive ? Color.gray : Color.white; 
            }
        }
    }
}


