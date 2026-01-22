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
        
        Invoke(nameof(StartPathfinding), 0.1f);
    }

    private void StartPathfinding()
    {
        var gameManager = GameManager.Instance;
        if (gameManager == null) return;

        MapContext map = gameManager.Context.map;
        GridData gridData = GridManager.Instance.Data;

        if (currentNode == null)
        {
            currentNode = map.SpawnNode;
            transform.position = currentNode.WorldPosition;
        }

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
        Debug.Log("Enemy reached the Core!");
        
        // 데이터가 있다면 데미지 수치 적용, 없으면 기본값 10
        float damage = data != null ? data.attackDamage : 10f;

        // TODO: GameManager.Instance.DamagePlayer(damage);

        Die(); 
    }
}
