using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Controllers")]
    [SerializeField] private GridController gridController;
    [SerializeField] private InputController inputController;
    [SerializeField] private UnitController unitController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private EconomyManager economyManager;

    //FSM
    private StateMachine<GameManager> stateMachine;
    public BaseState<GameManager> CurrentState => stateMachine.CurrentState;

    public CameraController Camera => cameraController;
    public InputController Input => inputController;

    protected override void Awake()
    {
        base.Awake();

        if (inputController == null) inputController = GetComponentInChildren<InputController>();
        if (cameraController == null) cameraController = GetComponentInChildren<CameraController>();
        if (economyManager == null) economyManager = GetComponentInChildren<EconomyManager>();

        stateMachine = new StateMachine<GameManager>(this);
    }

    void Start()
    {
        if (gridController != null) GridManager.Instance.Initialize(gridController);
        else Debug.LogError("GridController 누락됨!");

        if (unitController != null) UnitManager.Instance.Initialize(unitController);
        else Debug.LogError("UnitController 누락됨!");

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += HandleNodeClick;
            InputManager.Instance.OnRightClickNode += HandleRightClick;
        }
        else Debug.LogError("InputManager 누락됨!");

        ChangeState(new NormalState());
        Debug.Log("[GameManager] 초기화 완료.");
    }

    public void ChangeState(BaseState<GameManager> newState)
    {
        stateMachine.ChangeState(newState);
    }

    private void HandleNodeClick(GridNode node)
    {
        if (CurrentState is GameState gameState)
        {
            gameState.OnClickNode(node);
        }
    }

    private void HandleRightClick(GridNode node)
    {
        if (CurrentState is GameState gameState)
        {
            gameState.OnCancel();
        }
    }

    void Update()
    {
        stateMachine.Update();
    }
    
    public void GameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0; 
    }
}