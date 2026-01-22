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

    // FSM
    protected StateMachine stateMachine;

    // Path & Movement
    protected Node currentNode;
    protected List<Node> currentPath;
    protected int pathIndex;

    // Combat
    protected Unit targetUnit;
    protected float lastAttackTime;

    // Properties for States
    public Team MyTeam => data != null ? data.team : Team.Enemy;
    public bool IsDead => currentHp <= 0;
    public Node CurrentNode => currentNode;
    public UnitDataSO Data => data;
    
    // States need access to these
    public Unit TargetUnit 
    { 
        get => targetUnit; 
        set => targetUnit = value; 
    }

    protected virtual void Start()
    {
        InitializeStats();
        
        stateMachine = new StateMachine();
        stateMachine.ChangeState(new UnitIdleState(this));
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
        if (IsDead) return;

        stateMachine.Update();
    }

    #region FSM Interface (Methods called by States)

    public void ChangeState(IState newState)
    {
        stateMachine.ChangeState(newState);
    }

    // Called by MoveState. Returns true if path is finished.
    public bool MoveTick()
    {
        if (currentPath == null || pathIndex >= currentPath.Count)
        {
            return true; // Path complete
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

        // 경로의 끝에 도달했는지 확인
        return pathIndex >= currentPath.Count;
    }

    public bool HasPathRemaining()
    {
        return currentPath != null && pathIndex < currentPath.Count;
    }

    // Called by AttackState
    public void TryAttack(Unit target)
    {
        float cooldown = data != null ? data.attackCooldown : 1f;
        if (Time.time >= lastAttackTime + cooldown)
        {
            PerformAttack(target);
            lastAttackTime = Time.time;
        }
    }

    protected virtual void PerformAttack(Unit target)
    {
        float damage = data != null ? data.attackDamage : 0f;
        target.TakeDamage(damage);
        Debug.Log($"{name} attacked {target.name} for {damage} damage.");
    }
    
    // Abstract method implemented by Ally/Enemy Unit
    public abstract bool DetectTarget(out Unit target);

    #endregion

    #region Public Control Methods

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
        
        // 이동 명령이 내려오면 즉시 MoveState로 전환
        stateMachine.ChangeState(new UnitMoveState(this));
    }

    public void SetData(UnitDataSO newData)
    {
        this.data = newData;
        InitializeStats();
    }

    #endregion

    #region Internal Helpers

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
    
    protected virtual void OnPathComplete()
    {
        // State handles logic, but this callback can remain for specific events
    }

    #endregion
}
