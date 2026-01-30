using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 타일 오브젝트에 부착되어 효과 및 로직 담당
public class GridTile : MonoBehaviour
{
    [SerializeField] private TileDataSO data;
    public TileDataSO Data => data;

    public void Setup(TileDataSO data)
    {
        this.data = data;
    }
}