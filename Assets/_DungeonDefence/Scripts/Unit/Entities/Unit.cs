using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour, IPoolable
{
    [Header("Data")]
    [SerializeField] private UnitDataSO data;

    [Header("Components")]
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

    private StateMachine<Unit> stateMachine;
    public StateMachine<Unit> FSM => stateMachine;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<UnitMovement>();
        if (combat == null) combat = GetComponent<UnitCombat>();
        if (modelRenderer == null) modelRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Initialize(UnitDataSO data, GridNode startNode)
    {
        this.data = data;
        IsDead = false;

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
            transform.position = startNode.WorldPosition;
            CurrentNode = startNode;
            if (IsPlayerTeam && startNode.CurrentTileData != null && startNode.CurrentTileData.IsDispatchTile)
            {
                SetDispatchMode(true);
            }
            else
            {
                SetDispatchMode(false);
            }
        }

        UnitManager.Instance.RegisterUnit(this);
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
        if (UnitManager.Instance != null)
            UnitManager.Instance.UnregisterUnit(this);
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
    }

    public void OnReachTile(Vector2Int coord)
    {
        GridNode node = GridManager.Instance.GetNode(coord.x, coord.y);
        if (node != null)
        {
            SetNode(node);
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
    }

    private void HandleDeath()
    {
        IsDead = true;

        if (data.category == UnitCategory.Core)
        {
            Debug.Log($"[Unit] Core has been destroyed.");
        }

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Push(this);
        }
        else
        {
            OnDespawn();
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (UnitManager.Instance != null)
            UnitManager.Instance.UnregisterUnit(this);
    }

    private void SetDispatchMode(bool enable)
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

            Debug.Log($"[Unit] {Data.Name} 파견 업무 시작 (전투 제외됨)");
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
}
