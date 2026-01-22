using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    private GridSystem gridSystem;

    protected override void Start()
    {
        base.Start();
        gridSystem = new GridSystem(); 
        Invoke(nameof(StartPathfinding), 0.1f);
    }

    private void StartPathfinding()
    {
        var gameManager = GameManager.Instance;
        if (gameManager == null) return;

        MapContext map = gameManager.Context.map;
        
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
        // TODO: Player Damage Logic
        Die(); 
    }

    public override bool DetectTarget(out Unit target)
    {
        // 적 유닛은 아군 유닛을 공격할 수도 있고, 무시하고 코어로 갈 수도 있음.
        // 여기서는 "Attack Move" 로직을 따르므로, 경로 상의 아군 유닛을 공격한다고 가정.

        target = null;
        if (data == null) return false;

        AllyUnit[] allies = FindObjectsOfType<AllyUnit>();
        
        float minDst = float.MaxValue;
        float range = data.attackRange; // 적 유닛 데이터의 사거리 사용

        foreach (var ally in allies)
        {
            if (ally.IsDead) continue;

            float dst = Vector3.Distance(transform.position, ally.transform.position);
            
            if (dst <= range && dst < minDst)
            {
                minDst = dst;
                target = ally;
            }
        }

        return target != null;
    }
}