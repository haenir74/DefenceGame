using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    Ally,
    Enemy
}

public abstract class Unit : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] protected float maxHp = 100f;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float currentHp;
    [SerializeField] protected Team team;

    protected Node currentNode;
    protected List<Node> currentPath;
    protected int pathIndex;
    protected bool isMoving;

    public Team MyTeam => team;
    public bool IsDead => currentHp <= 0;
    public Node CurrentNode => currentNode;

    protected virtual void Start()
    {
        currentHp = maxHp;
    }

    protected virtual void Update()
    {
        if (isMoving && currentPath != null)
        {
            MoveAlongPath();
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHp -= damage;
        
        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} (Team: {team}) Died.");
        Destroy(gameObject);
    }

    // 즉시 이동 (텔레포트)
    public void SetNode(Node newNode)
    {
        HandleNodeChange(currentNode, newNode);
        currentNode = newNode;
        transform.position = newNode.WorldPosition;
    }

    // 경로 설정 및 이동 시작
    public void SetPath(List<Node> path)
    {
        if (path == null || path.Count == 0) return;
        currentPath = path;
        pathIndex = 0;
        isMoving = true;
    }

    private void MoveAlongPath()
    {
        if (pathIndex >= currentPath.Count)
        {
            isMoving = false;
            OnPathComplete();
            return;
        }

        Node targetNode = currentPath[pathIndex];
        Vector3 targetPos = targetNode.WorldPosition;
        
        // y값은 현재 높이 유지 (혹은 0으로 고정)
        targetPos.y = transform.position.y;

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            // 노드 도달 시 업데이트
            HandleNodeChange(currentNode, targetNode);
            currentNode = targetNode;
            pathIndex++;
        }
    }

    private void HandleNodeChange(Node prev, Node next)
    {
        if (prev != null && prev.TileEffect != null)
        {
            prev.TileEffect.OnUnitExit(this);
        }

        if (next != null && next.TileEffect != null)
        {
            next.TileEffect.OnUnitEnter(this);
        }
    }

    protected virtual void OnPathComplete()
    {
        // 목적지 도착 시 호출
    }
}