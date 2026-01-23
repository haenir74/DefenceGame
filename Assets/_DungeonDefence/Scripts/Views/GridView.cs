using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridView : MonoBehaviour
{
    private MapContext _context;
    private GridData _data;

    public void Setup(MapContext context, GridData data)
    {
        _context = context;
        _data = data;
    }

    private void OnDrawGizmos()
    {
        if (_data == null) return;

        Gizmos.color = Color.green;
        
        int w = (_context != null) ? _context.Width : _data.width;
        int h = (_context != null) ? _context.Height : _data.height;
        float size = _data.cellSize;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Vector3 worldPos = new Vector3(x * size, 0, y * size);
                Gizmos.DrawWireCube(worldPos, new Vector3(size, 0.1f, size));
            }
        }
        
        Gizmos.color = Color.blue;
        Vector3 spawnPos = new Vector3(_data.spawnNodePos.x * size, 0, _data.spawnNodePos.y * size);
        Gizmos.DrawWireSphere(spawnPos, size * 0.3f);

        Gizmos.color = Color.red;
        Vector3 corePos = new Vector3(_data.coreNodePos.x * size, 0, _data.coreNodePos.y * size);
        Gizmos.DrawWireSphere(corePos, size * 0.3f);
    }
}