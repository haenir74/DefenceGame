using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] UnitDataSO coreUnit;

    [Header("Controllers")]
    [SerializeField] private InputController inputController;

    private StateMachine<GameManager> stateMachine;
    public BaseState<GameManager> CurrentState => stateMachine?.CurrentState;

    public int CurrentWave { get; private set; } = 1;
    public UnitDataSO SelectedUnitToPlace { get; private set; }
    public TileDataSO SelectedTileToPlace { get; private set; }
    public bool IsMaintenancePhase => CurrentState is MaintenanceState;

    protected override void Awake()
    {
        base.Awake();
        if (this.inputController == null) this.inputController = FindObjectOfType<InputController>();
        this.stateMachine = new StateMachine<GameManager>(this);
    }

    private void Start()
    {
        InitializeSystems();
        ChangeState(new MaintenanceState(this));

        StartCoroutine(StartCo());
    }

    private IEnumerator StartCo()
    {
        yield return null;
        while (GridManager.Instance == null || GridManager.Instance.GetCoreNode() == null)
        {
            yield return null;
        }

        // 카메라 초기 위치를 그리드 중앙으로 설정
        int centerX = (GridManager.Instance.Data.width - 1) / 2;
        int centerY = (GridManager.Instance.Data.height - 1) / 2;
        GridNode centerNode = GridManager.Instance.GetNode(centerX, centerY);
        if (centerNode != null)
        {
            // SmoothDamp를 타기 위해 즉시 이동이 아닌 FocusOn 호출
            FocusCamera(centerNode);
        }

        SpawnCore();
        EconomyManager.Instance.AddCurrency(CurrencyType.Gold, 500); // 사용자 요청대로 500G 지급
    }

    private void Update()
    {
        stateMachine?.Update();
    }

    private void InitializeSystems()
    {
        if (GridManager.Instance != null)
            GridManager.Instance.Initialize();

        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.Initialize();
            UnitManager.Instance.OnUnitDead += HandleUnitDead;
        }

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += HandleNodeClick;
            InputManager.Instance.OnRightClickNode += HandleRightClick;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode -= HandleNodeClick;
            InputManager.Instance.OnRightClickNode -= HandleRightClick;
        }
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnUnitDead -= HandleUnitDead;
        }
    }

    private void HandleUnitDead(Unit unit)
    {
        if (unit != null && unit.Data.category == UnitCategory.Core)
        {
            GameOver();
        }
    }

    // Input handlers
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

    // FSM
    public void ChangeState(BaseState<GameManager> newState)
    {
        stateMachine?.ChangeState(newState);
    }

    // ** Game Flow **
    public void StartBattlePhase()
    {
        if (IsMaintenancePhase)
        {
            ClearSelection();
            int waveIndex = CurrentWave - 1;
            var battleDTO = new BattleStateDTO(waveIndex);
            ChangeState(new BattleState(this, battleDTO));
        }
    }

    public void EndBattlePhase(bool isWin = true)
    {
        if (isWin)
        {
            if (RewardManager.Instance != null)
            {
                RewardManager.Instance.ProcessWaveClear(CurrentWave);
            }
            else
            {
                EconomyManager.Instance?.AddCurrency(CurrencyType.Gold, 100);
                SwitchToMaintenancePhase();
            }
        }
        else
        {
            GameOver();
        }
    }

    public void SwitchToMaintenancePhase()
    {
        CurrentWave++;
        if (CurrentWave > 30)
        {
            Victory();
            return;
        }
        ChangeState(new MaintenanceState(this));
    }

    public void Victory()
    {
        stateMachine?.ChangeState(new VictoryState(this));
        Time.timeScale = 0;
        Debug.Log("<color=yellow>[GameManager] 게임 클리어! 30웨이브 승리!</color>");
        // TODO: VictoryScene으로 씬 전환
        // SceneManager.LoadScene("VictoryScene");
    }

    public void GameOver()
    {
        // 현재 state의 OnExit 실행 (이벤트 구독 해제 포함)
        stateMachine?.ChangeState(new GameOverState(this));
        Time.timeScale = 0;
        Debug.Log("[GameManager] Game Over!");
    }
    // ***

    // ** Select System **
    public void SelectUnitToPlace(UnitDataSO unitData)
    {
        if (!IsMaintenancePhase) return;

        SelectedUnitToPlace = unitData;
        SelectedTileToPlace = null;

        Debug.Log($"[GameManager] 유닛 선택: {unitData.Name}");
    }

    public void SelectTileToPlace(TileDataSO tileData)
    {
        if (!IsMaintenancePhase) return;

        SelectedTileToPlace = tileData;
        SelectedUnitToPlace = null;

        Debug.Log($"[GameManager] 타일 선택: {tileData.Name}");
    }

    public void ClearSelection()
    {
        SelectedUnitToPlace = null;
        SelectedTileToPlace = null;
    }

    /// <summary>
    /// 드래그 앤 드롭으로 그리드 배치 시 호출.
    /// SelectUnitToPlace 이후에 호출해야 하며, MaintenanceState의 배치 로직을 재사용.
    /// </summary>
    public void TryPlaceSelectedUnit(GridNode node)
    {
        if (!IsMaintenancePhase || node == null) return;
        if (CurrentState is MaintenanceState ms)
        {
            ms.OnClickNode(node);
        }
    }
    // ***

    // ** Helper **
    private void SpawnCore()
    {
        if (coreUnit == null) return;
        GridNode coreNode = GridManager.Instance.GetCoreNode();
        if (coreNode != null)
        {
            UnitManager.Instance.SpawnUnit(coreUnit, coreNode);
        }
    }

    public bool IsInState<T>() where T : GameState
    {
        return CurrentState is T;
    }

    // ** CameraManager **
    public void FocusCamera(Vector3 targetPosition) => CameraManager.Instance?.FocusOn(targetPosition);
    public void FocusCamera(GridNode node) { if (node != null) FocusCamera(node.WorldPosition); }
    public void ResetCamera() => CameraManager.Instance?.ResetPosition();
    // ***
}