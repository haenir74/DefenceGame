using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Tile : MonoBehaviour
{
    public Node node;
    public TileData data;

    [SerializeField] protected MeshRenderer meshRenderer;
    protected Color originalColor;

    public void Setup(Node node, TileData data)
    {
        this.node = node;
        this.data = data;
        transform.position = node.WorldPosition;
        node.CurrentTile = this;
        
        if(meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        originalColor = meshRenderer.material.color;

        OnPlaced();
    }

    public void OnHoverEnter() => meshRenderer.material.color = originalColor * 0.8f;
    public void OnHoverExit() => meshRenderer.material.color = originalColor;
    
    protected abstract void OnPlaced();
    public abstract void OnStepped();
}