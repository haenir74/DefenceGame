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
        this.CurrentNode = startNode;

        if (movement != null) movement.Setup(this);
        
        if (combat != null) combat.Setup(this, data);

        if (startNode != null)
        {
            transform.position = startNode.WorldPosition;
        }

        UnitManager.Instance.RegisterUnit(this);

        stateMachine = new StateMachine<Unit>(this);
        stateMachine.ChangeState(new UnitIdleState());
    }

    public void SetNode(GridNode newNode)
    {
        CurrentNode = newNode;
    }

    private void Update()
    {
        stateMachine?.Update();
    }

    private void OnDestroy()
    {
        if (UnitManager.Instance != null)
            UnitManager.Instance.UnregisterUnit(this);
    }
}
