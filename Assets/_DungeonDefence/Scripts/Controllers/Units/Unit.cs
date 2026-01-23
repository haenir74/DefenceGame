using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Panex.Stat.Model;
using Panex.Stat.Controller;

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitDataSO _data;
    [SerializeField] private UnitState _currentState;

    private UnitMovement _movement;
    private UnitCombat _combat;
    private StateMachine _fsm;
    private float _currentHp;

    public UnitDataSO Data => _data;
    public Team MyTeam => _data != null ? _data.team : Team.Enemy;
    public Node CurrentNode => _movement?.CurrentNode;
    public bool IsDead => _currentHp <= 0;

    public Unit TargetUnit { get; set; }
    public IState CurrentState => _fsm.CurrentState;

    private void Awake()
    {
        _movement = new UnitMovement(this);
        _combat = new UnitCombat(this);
        _fsm = new StateMachine();
    }

    private void Start()
    {
        _combat.Initialize();
        _movement.Initialize();

        if (_data != null)
        {
            InitializeUnit(_data);
        }

        _fsm.ChangeState(new UnitIdleState(this));
    }

    private void Update()
    {
        if (IsDead) return;
        _fsm.Update();
    }

    public void InitializeUnit(UnitDataSO data)
    {
        _data = data;
        _combat.SyncStats(data);
        _currentHp = _combat.MaxHP;
    }

    public void ChangeState(IState newState) => _fsm.ChangeState(newState);
    public void SetStateEnum(UnitState s) => _currentState = s;

    public bool MoveTick() => _movement.Tick();
    public bool HasPathRemaining() => _movement.HasPendingMove;
    public void DecideNextMove() => _movement.DecideNextMove();
    public void SetNode(Node node) => _movement.SetNode(node);
    public void SetAllyDestination(Node dest) => _movement.SetAllyDestination(dest);

    public void TryAttack(Unit target) => _combat.TryAttack(target);
    public bool DetectTarget(out Unit target) => _combat.DetectTarget(out target);
    
    public void ReduceHealth(float amount)
    {
        _currentHp -= amount;
        if (_currentHp <= 0) Die();
    }

    public void TakeDamage(float amount, bool isCrit) => _combat.TakeDamage(amount, isCrit);
    public void TakeDamage(float amount) => TakeDamage(amount, false);

    private void Die()
    {
        _movement.SetNode(null);

        if (_data != null && _data.isCore)
        {
            GameManager.Instance.GameOver();
        }

        Destroy(gameObject);
    }
}