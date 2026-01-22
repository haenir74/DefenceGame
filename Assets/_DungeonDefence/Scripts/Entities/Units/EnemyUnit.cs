using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    private GridSystem gridSystem;

    protected override void Start()
    {
        base.Start();
        gridSystem = new GridSystem(); // In a real architecture, inject this or use a singleton wrapper
        
        // 잠시 대기 후 이동 시작 (생성 직후 바로 경로 탐색이 안될 수도 있으므로)
        Invoke(nameof(StartPathfinding), 0.1f);
    }

    private void StartPathfinding()
    {
        var gameManager = GameManager.Instance;
        if (gameManager == null) return;

        MapContext map = gameManager.Context.map;
        GridData data = GridManager.Instance.Data;

        // 현재 노드가 없으면 스폰 노드로 가정 (혹은 외부에서 SetNode로 초기화 필요)
        if (currentNode == null)
        {
            currentNode = map.SpawnNode;
            transform.position = currentNode.WorldPosition;
        }

        // 경로 계산: 현재 위치 -> 플레이어 코어
        List<Node> path = gridSystem.FindPath(map, currentNode, map.CoreNode);
        
        if (path != null)
        {
            SetPath(path);
        }
        else
        {
            Debug.LogWarning("Enemy cannot find path to Core!");
        }
    }

    protected override void OnPathComplete()
    {
        // 코어 도달!
        Debug.Log("Enemy reached the Core!");
        
        // TODO: 플레이어 체력 감소 로직
        // GameManager.Instance.DamagePlayer(damageAmount);

        // 자폭
        Die(); 
    }
}
