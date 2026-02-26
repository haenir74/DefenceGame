using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour, IPoolable
{
    
    [SerializeField] private UnitDataSO data;

    
    [SerializeField] private UnitMovement movement;
    [SerializeField] private UnitCombat combat;
    [SerializeField] private SpriteRenderer modelRenderer;

    public UnitDataSO Data => data;
    public UnitMovement Movement => movement;
    public UnitCombat Combat => combat;

    public GridNode CurrentNode { get; private set; }
    public Vector2Int Coordinate => CurrentNode != null ? CurrentNode.Coordinate : Vector2Int.zero;

    public bool IsDead { get; private set; }
    public bool IsDispatched { get; private set; }
    public bool IsPlayerTeam => data != null && data.isPlayerTeam;

    public bool IsTargetable => !IsDead && !IsDispatched;
    public List<Vector2Int> VisitedHistory { get; private set; } = new List<Vector2Int>();

    private StateMachine<Unit> stateMachine;
    public StateMachine<Unit> FSM => stateMachine;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<UnitMovement>();
        if (combat == null) combat = GetComponent<UnitCombat>();
        if (modelRenderer == null) modelRenderer = GetComponentInChildren<SpriteRenderer>();

        
        if (GetComponent<UnitVisualController>() == null)
            gameObject.AddComponent<UnitVisualController>();
    }

    public void Initialize(UnitDataSO data, GridNode startNode)
    {
        if (data != null) this.data = data;

        IsDead = false;

        if (movement == null) movement = GetComponent<UnitMovement>();
        if (combat == null) combat = GetComponent<UnitCombat>();
        if (modelRenderer == null) modelRenderer = GetComponentInChildren<SpriteRenderer>();

        if (movement != null) movement.Initialize(this);
        if (combat != null)
        {
            combat.Initialize(this, data);
            combat.OnDeath -= HandleDeath;
            combat.OnDeath += HandleDeath;
        }

        stateMachine = new StateMachine<Unit>(this);

        if (startNode != null)
        {
            CurrentNode = startNode;

            if (data != null && data.category == UnitCategory.Core)
            {
                
                Vector3 center = startNode.WorldPosition;
                transform.position = new Vector3(center.x, UnitConstants.UNIT_HEIGHT, center.z);
                
            }
            else
            {
                
                Vector3 cur = transform.position;
                transform.position = new Vector3(cur.x, UnitConstants.UNIT_HEIGHT, cur.z);
            }

            if (movement != null) movement.Initialize(this);
            
            SetDispatchMode(false);

        }

        
        
        var visualController = GetComponent<UnitVisualController>();
        if (visualController != null) visualController.Apply();

        var animator = GetComponentInChildren<UnitSpriteAnimator>();
        if (animator != null) animator.Initialize(this, modelRenderer);

        var mecanimAnimator = GetComponentInChildren<UnitMecanimAnimator>();
        if (mecanimAnimator != null) mecanimAnimator.Initialize(this);

        VisitedHistory.Clear();
        if (startNode != null) VisitedHistory.Add(startNode.Coordinate);

        if (IsPlayerTeam)
            stateMachine.ChangeState(new UnitIdleState(this));
        else
            stateMachine.ChangeState(new EnemyTurnState(this));
    }

    public void OnSpawn()
    {
        gameObject.SetActive(true);
    }

    public void OnDespawn()
    {
        
        IsDead = true;
        gameObject.SetActive(false);
    }

    public void SetNode(GridNode newNode)
    {
        GridNode oldNode = CurrentNode;
        CurrentNode = newNode;

        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.MoveUnit(this, oldNode, newNode);
        }

        AddVisitedNode(newNode.Coordinate);
    }

    private void AddVisitedNode(Vector2Int coord)
    {
        VisitedHistory.Add(coord);
        
        if (VisitedHistory.Count > 20) VisitedHistory.RemoveAt(0);
    }

    public void OnReachTile(Vector2Int coord)
    {
        GridNode newNode = GridManager.Instance.GetNode(coord.x, coord.y);
        if (newNode != null)
        {
            
            if (CurrentNode != null && CurrentNode.Tile != null && !CurrentNode.Equals(newNode))
            {
                CurrentNode.Tile.OnUnitExit(this);
            }

            SetNode(newNode);

            
            if (CurrentNode.Tile != null)
            {
                CurrentNode.Tile.OnUnitEnter(this);
            }
        }
        CheckCombatCondition();
    }

    public void CheckCombatCondition()
    {
        if (IsDead || IsDispatched) return;
        Unit target = UnitManager.Instance.GetOpponentAt(Coordinate, IsPlayerTeam);
        if (target != null)
        {
            StartCombat();
        }
        else
        {
            (stateMachine.CurrentState as UnitState)?.OnStepFinished();
        }
    }

    public void OnWaveClear()
    {
        if (CurrentNode?.Tile != null)
        {
            CurrentNode.Tile.OnWaveClear(this);
        }
    }

    public void StartCombat()
    {
        if (IsDispatched) return;
        if (stateMachine.CurrentState is UnitCombatState) return;

        stateMachine.ChangeState(new UnitCombatState(this));
    }

    public void EndCombat()
    {
        if (!(stateMachine.CurrentState is UnitCombatState)) return;
        if (IsPlayerTeam)
            stateMachine.ChangeState(new UnitIdleState(this));
        else
            stateMachine.ChangeState(new EnemyTurnState(this));
    }

    public void OnUpdate()
    {
        if (IsDead) return;

        stateMachine?.Update();
        if (combat != null) combat.OnUpdate();
        if (movement != null) movement.OnUpdate();

        
        if (data != null && data.skill != null)
        {
            data.skill.OnUnitUpdate(this);
        }

        
        if (!IsDispatched && CurrentNode?.Tile != null)
        {
            CurrentNode.Tile.OnUnitUpdate(this);
        }
    }

    private void HandleDeath()
    {
        if (IsDead) return; 
        IsDead = true;

        
        if (movement != null && movement.IsMoving)
            movement.CancelMove();

        
        if (CurrentNode?.Tile != null)
        {
            CurrentNode.Tile.OnUnitDeath(this);
        }

        
        
        if (data != null && data.skill != null)
        {
            data.skill.OnUnitDie(this);
        }

        
        CurrentNode?.ReleaseSlot(this);

        if (data != null && data.category == UnitCategory.Core)
        {
            
        }

        
        if (UnitManager.Instance != null)
            UnitManager.Instance.UnregisterUnit(this);

        if (PoolManager.Instance != null && gameObject.activeInHierarchy)
        {
            PoolManager.Instance.Push(this);
        }
        else
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        
        if (!IsDead && UnitManager.Instance != null)
            UnitManager.Instance.UnregisterUnit(this);
    }

    public void SetDispatchMode(bool enable)
    {
        IsDispatched = enable;

        if (enable)
        {
            if (combat != null) combat.enabled = false;
            if (modelRenderer != null)
            {
                Color color = modelRenderer.color;
                color.a = 0.5f;
                modelRenderer.color = color;
            }

            
        }
        else
        {
            if (combat != null) combat.enabled = true;
            if (modelRenderer != null)
            {
                Color color = modelRenderer.color;
                color.a = 1.0f;
                modelRenderer.color = color;
            }
        }
    }

    
    
    
    
    public void SetVisualVisible(bool visible)
    {
        if (modelRenderer != null) modelRenderer.enabled = visible;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = visible;

        
    }
}



