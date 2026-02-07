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
    [SerializeField] private EnemyPathFinder pathFinder;

    public UnitDataSO Data => data;
    public UnitMovement Movement => movement;
    public UnitCombat Combat => combat;

    public GridNode CurrentNode { get; private set; }
    public Vector2Int Coordinate => CurrentNode != null ? CurrentNode.Coordinate : Vector2Int.zero;

    public bool IsDead { get; private set; }
    public bool IsPlayerTeam => data != null && data.isPlayerTeam;

    private StateMachine<Unit> stateMachine;
    public StateMachine<Unit> FSM => stateMachine;

    private void Awake()
    {
        if (this.movement == null) this.movement = GetComponent<UnitMovement>();
        if (this.combat == null) this.combat = GetComponent<UnitCombat>();
        if (this.pathFinder == null) this.pathFinder = GetComponent<EnemyPathFinder>();
    }

    public void Initialize(UnitDataSO data, GridNode startNode)
    {
        this.data = data;
        IsDead = false;

        if (movement != null) movement.Initialize(this);
        {
            combat.Initialize(this, data);
            combat.OnDeath -= HandleDeath;
            combat.OnDeath += HandleDeath;
        }

        stateMachine = new StateMachine<Unit>(this);

        if (startNode != null)
        {
            transform.position = startNode.WorldPosition;
            SetNode(startNode); 
        }

        if (IsPlayerTeam)
            stateMachine.ChangeState(new UnitIdleState());
        else
            stateMachine.ChangeState(new EnemyTurnState());

        UnitManager.Instance.RegisterUnit(this);
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

        if (pathFinder != null)
        {
            pathFinder.OnReachTile();
        }
    }

    public void OnReachTile(Vector2Int coord)
    {
        GridNode node = GridManager.Instance.GetNode(coord.x, coord.y);
        if (node != null)
        {
            SetNode(node);
        }
    }

    public void StartCombat()
    {
        if (stateMachine.CurrentState is UnitCombatState) return;
        stateMachine.ChangeState(new UnitCombatState());
    }

    public void EndCombat()
    {
        if (!(stateMachine.CurrentState is UnitCombatState)) return;
        if (IsPlayerTeam)
            stateMachine.ChangeState(new UnitIdleState());
        else
            stateMachine.ChangeState(new EnemyTurnState());
    }

    public void OnUpdate()
    {
        if (IsDead) return;

        stateMachine?.Update();

        if (!IsPlayerTeam && movement != null && !movement.IsMoving)
        {
            if (!(stateMachine.CurrentState is UnitCombatState))
            {
                if (pathFinder != null)
                {
                    pathFinder.FindNextStep();
                    Vector2Int? nextStep = pathFinder.GetTargetStep();

                    if (nextStep.HasValue)
                    {
                        movement.MoveTo(nextStep.Value);
                    }
                }
            }
        }

        if (movement != null) movement.OnUpdate();
    }

    private void HandleDeath()
    {
        IsDead = true;
        
        if (data.category == UnitCategory.Core)
        {
            Debug.Log($"GAME OVER");
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
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

}
