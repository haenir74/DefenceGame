using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitDataSO data;

    [Header("Components")]
    [SerializeField] private UnitMovement movement;
    [SerializeField] private UnitCombat combat;

    public UnitDataSO Data => data;
    public UnitMovement Movement => movement;
    public UnitCombat Combat => combat;
    public GridNode CurrentNode { get; private set; }

    private StateMachine<Unit> stateMachine;
    public StateMachine<Unit> FSM => stateMachine;
    public bool IsPlayerTeam => data != null && data.isPlayerTeam;

    public void Setup(UnitDataSO data, GridNode startNode)
    {
        this.data = data;

        if (movement != null) movement.Setup(this);
        if (combat != null) {
            combat.Setup(this, data);

            combat.OnDeath -= HandleDeath;
            combat.OnDeath += HandleDeath;
        }

        var pathfinder = GetComponent<EnemyPathfinder>();
        if (pathfinder == null) pathfinder = gameObject.AddComponent<EnemyPathfinder>();
        pathfinder.Initialize(this);

        stateMachine = new StateMachine<Unit>(this);
        UnitManager.Instance.RegisterUnit(this);

        if (startNode != null)
        {
            transform.position = startNode.WorldPosition;
            SetNode(startNode);
        }

        if (stateMachine.CurrentState == null)
        {
            if (IsPlayerTeam)
                stateMachine.ChangeState(new UnitIdleState());
            else
                stateMachine.ChangeState(new EnemyTurnState());
        }
    }

    public void SetNode(GridNode newNode)
    {
        if (CurrentNode != null) CurrentNode.OnUnitExit(this);
        CurrentNode = newNode;
        if (CurrentNode != null) CurrentNode.OnUnitEnter(this);
    }

    public void StartCombat()
    {
        if (FSM.CurrentState is UnitCombatState) return;
        FSM.ChangeState(new UnitCombatState());
    }

    public void EndCombat()
    {
        if (FSM == null) return;
        if (!(FSM.CurrentState is UnitCombatState)) return;

        if (IsPlayerTeam)
            stateMachine.ChangeState(new UnitIdleState());
        else
            stateMachine.ChangeState(new EnemyTurnState());
    }

    private void HandleDeath()
    {
        if (data.category == UnitCategory.Core)
        {
            Debug.Log($"<color=red>🚨 [Unit] CORE DESTROYED! GAME OVER! 🚨</color>");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    private void Update()
    {
        stateMachine?.Update();
    }

    private void OnDestroy()
    {
        if (CurrentNode != null)
            CurrentNode.OnUnitExit(this);
            
        if (UnitManager.Instance != null)
            UnitManager.Instance.UnregisterUnit(this);
    }
}
