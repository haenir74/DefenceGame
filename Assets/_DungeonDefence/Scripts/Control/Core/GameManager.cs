using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Controllers")]
    [SerializeField] private GridController gridController;
    [SerializeField] private InputController inputController;

    //FSM
    private StateMachine<GameManager> stateMachine;
    public BaseState<GameManager> CurrentState => stateMachine.CurrentState;

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new StateMachine<GameManager>(this);
    }

    void Start()
    {
        if (gridController != null)
        {
            GridManager.Instance.Initialize(gridController);
        }

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += HandleNodeClick;
            InputManager.Instance.OnRightClickNode += HandleRightClick;
        }

        ChangeState(new NormalState());
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