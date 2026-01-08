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
        if (_context == null || _data == null) return;

        Gizmos.color = Color.green;
        for (int x = 0; x < _data.width; x++)
        {
            for (int y = 0; y < _data.height; y++)
            {
                Vector3 worldPos = new Vector3(x * _data.cellSize, 0, y * _data.cellSize);
                Gizmos.DrawWireCube(worldPos, new Vector3(_data.cellSize, 0.1f, _data.cellSize));
            }
        }
    }
}