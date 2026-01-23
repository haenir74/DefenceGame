using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridView : MonoBehaviour
{
    private MapContext context;
    private GridData data;

    public void Setup(MapContext context, GridData data)
    {
        this.context = context;
        this.data = data;
    }

    private void OnDrawGizmos()
    {
        if (data == null) return;

        Gizmos.color = Color.green;
        
        int w = (context != null) ? context.Width : data.width;
        int h = (context != null) ? context.Height : data.height;
        float size = data.cellSize;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Vector3 worldPos = new Vector3(x * size, 0, y * size);
                Gizmos.DrawWireCube(worldPos, new Vector3(size, 0.1f, size));
            }
        }
        
        Gizmos.color = Color.blue;
        Vector3 spawnPos = new Vector3(data.spawnNodePos.x * size, 0, data.spawnNodePos.y * size);
        Gizmos.DrawWireSphere(spawnPos, size * 0.3f);

        Gizmos.color = Color.red;
        Vector3 corePos = new Vector3(data.coreNodePos.x * size, 0, data.coreNodePos.y * size);
        Gizmos.DrawWireSphere(corePos, size * 0.3f);
    }
}