using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private GridController gridController;
    [SerializeField] private UnitController unitController;
    [SerializeField] private InputController inputController;

    private StateMachine<GameController> stateMachine;
    public BaseState<GameController> CurrentState => stateMachine?.CurrentState;

    private void Awake()
    {
        if (this.gridController == null) this.gridController = GetComponentInChildren<GridController>();
        if (this.unitController == null) this.unitController = GetComponentInChildren<UnitController>();
        if (this.inputController == null) this.inputController = FindObjectOfType<InputController>();
        
        this.stateMachine = new StateMachine<GameController>(this);
    }

    private void Start()
    {
        InitializeSystems();
        ChangeState(new MaintenanceState(this));
    }

    private void Update()
    {
        stateMachine?.Update();
    }

    private void InitializeSystems()
    {
        if (GridManager.Instance != null && this.gridController != null)
            GridManager.Instance.Initialize(this.gridController);

        if (UnitManager.Instance != null && this.unitController != null)
            UnitManager.Instance.Initialize(this.unitController);

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += HandleNodeClick;
            InputManager.Instance.OnRightClickNode += HandleRightClick;
        }
    }

    // FSM
    public void ChangeState(BaseState<GameController> newState)
    {
        stateMachine.ChangeState(newState);
    }

    // Input
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

    // Game Flow

    public void GameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0; 
        // UI 팝업 등 추가 로직
    }
}