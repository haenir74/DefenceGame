using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Panex.Stat.Model;
using Panex.Stat.Controller;

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitDataSO data;
    [SerializeField] private UnitState currentState;

    private UnitMovement movement;
    private UnitCombat combat;
    private StateMachine fsm;
    private float currentHp;

    public UnitDataSO Data => data;
    public Team MyTeam => data != null ? data.team : Team.Enemy;
    public Node CurrentNode => movement?.CurrentNode;
    public bool IsDead => currentHp <= 0;
    public float AttackRange => combat.AttackRange;

    public Unit TargetUnit { get; set; }
    public IState CurrentState => fsm.CurrentState;

    private void Awake()
    {
        movement = new UnitMovement(this);
        combat = new UnitCombat(this);
        fsm = new StateMachine();
    }

    private void Start()
    {
        combat.Initialize();
        movement.Initialize();

        if (data != null)
        {
            InitializeUnit(data);
        }

        fsm.ChangeState(new UnitIdleState(this));
    }

    private void Update()
    {
        if (IsDead) return;
        fsm.Update();
    }

    public void InitializeUnit(UnitDataSO data)
    {
        this.data = data;
        combat.SyncStats(data);
        currentHp = combat.MaxHP;
    }

    public void ChangeState(IState newState) => fsm.ChangeState(newState);
    public void SetStateEnum(UnitState s) => currentState = s;

    public bool MoveTick() => movement.Tick();
    public bool HasPathRemaining() => movement.HasPendingMove;
    public void DecideNextMove() => movement.DecideNextMove();
    public void SetNode(Node node) => movement.SetNode(node);
    public void SetAllyDestination(Node dest) => movement.SetAllyDestination(dest);
    public void MoveTowards(Vector3 pos) => movement.MoveTowards(pos);
    public void UpdateStance() => movement.UpdateStance();

    public void TryAttack(Unit target) => combat.TryAttack(target);
    public bool DetectTarget(out Unit target) => combat.DetectTarget(out target);
    
    public void ReduceHealth(float amount)
    {
        currentHp -= amount;
        if (currentHp <= 0) Die();
    }

    public void TakeDamage(float amount, bool isCrit) => combat.TakeDamage(amount, isCrit);
    public void TakeDamage(float amount) => TakeDamage(amount, false);

    private void Die()
    {
        movement.SetNode(null);

        if (data != null && data.isCore)
        {
            GameManager.Instance.GameOver();
        }

        Destroy(gameObject);
    }
}
