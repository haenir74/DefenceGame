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
    [Header("Data")]
    [SerializeField] protected UnitDataSO data;

    [Header("Current Stats")]
    [SerializeField] protected float currentHp;

    protected Node currentNode;
    protected List<Node> currentPath;
    protected int pathIndex;
    protected bool isMoving;

    public Team MyTeam => data != null ? data.team : Team.Enemy; // Default safely
    public bool IsDead => currentHp <= 0;
    public Node CurrentNode => currentNode;
    public UnitDataSO Data => data; // Public accessor

    protected virtual void Start()
    {
        InitializeStats();
    }

    protected virtual void InitializeStats()
    {
        if (data != null)
        {
            currentHp = data.maxHp;
        }
        else
        {
            Debug.LogError($"{gameObject.name} is missing UnitDataSO!");
        }
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
        Debug.Log($"{gameObject.name} (Team: {MyTeam}) Died.");
        Destroy(gameObject);
    }

    public void SetNode(Node newNode)
    {
        HandleNodeChange(currentNode, newNode);
        currentNode = newNode;
        transform.position = newNode.WorldPosition;
    }

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
        targetPos.y = transform.position.y;

        float speed = data != null ? data.moveSpeed : 2f;
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
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
    }
    
    // 유닛 데이터를 런타임에 교체/설정할 수 있도록 메서드 추가
    public void SetData(UnitDataSO newData)
    {
        this.data = newData;
        InitializeStats();
    }
}
