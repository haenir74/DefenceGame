using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] UnitDataSO coreUnit;

    private StateMachine<GameManager> stateMachine;
    public BaseState<GameManager> CurrentState => stateMachine?.CurrentState;

    public enum SelectionSource { Inventory, Grid, Dispatch }
    public SelectionSource CurrentSelectionSource { get; private set; }
    public GridNode OriginalNode { get; private set; }
    public Unit PickedUpUnit { get; private set; }

    public int CurrentWave { get; private set; } = 1;
    public UnitDataSO SelectedUnitToPlace { get; private set; }
    public TileDataSO SelectedTileToPlace { get; private set; }
    public bool IsMaintenancePhase => CurrentState is MaintenanceState;

    public int TotalEnemiesKilled { get; private set; }
    public int TotalGoldEarned { get; private set; }
    public int TotalUnitsUsed { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        this.stateMachine = new StateMachine<GameManager>(this);
    }

    private void ResetPersistentSystems()
    {
        if (PoolManager.Instance != null) PoolManager.Instance.ClearPools();
        if (EconomyManager.Instance != null) EconomyManager.Instance.Reset();
        if (InventoryManager.Instance != null) InventoryManager.Instance.Reset();

        TierProbabilities stage1Probs = (WaveManager.Instance != null) ? WaveManager.Instance.GetNextWaveTierProbs() : null;
        if (ShopManager.Instance != null) ShopManager.Instance.ResetWithProbabilities(stage1Probs);

        if (UnitManager.Instance != null) UnitManager.Instance.Reset();
        if (WaveManager.Instance != null) WaveManager.Instance.Reset();
        if (DragDropManager.Instance != null) DragDropManager.Instance.CancelDrag();
        if (MetaManager.Instance != null) MetaManager.Instance.ResetRunResult();

        TotalEnemiesKilled = 0;
        TotalGoldEarned = 0;
        TotalUnitsUsed = 0;
        CurrentWave = 1;
    }

    public void Initialize()
    {
        ResetPersistentSystems();
        InitializeSystems();
        ChangeState(new MaintenanceState(this));

        if (GridManager.Instance != null && GridManager.Instance.Data != null)
        {
            int centerX = (GridManager.Instance.Data.width - 1) / 2;
            int centerY = (GridManager.Instance.Data.height - 1) / 2;
            GridNode centerNode = GridManager.Instance.GetNode(centerX, centerY);
            if (centerNode != null)
            {
                FocusCamera(centerNode);
            }
        }

        SpawnCore();

        int startGold = 500;
        if (MetaManager.Instance != null)
        {
            float bonus = MetaManager.Instance.GetPerkLevel("StartGold") * 100;
            startGold += (int)bonus;
        }

        EconomyManager.Instance?.AddCurrency(CurrencyType.Gold, startGold);
        GrantInitialItems();
    }

    private void GrantInitialItems()
    {
        if (InventoryManager.Instance == null) return;


        UnitDataSO normalSlime = Resources.Load<UnitDataSO>("Data/Units/Allies/Player_NormalSlime_Data");
        if (normalSlime != null)
        {
            InventoryManager.Instance.AddItem(normalSlime, 3);
        }


        TileDataSO stoneRoad = Resources.Load<TileDataSO>("Data/Tiles/Tile_StoneRoad");
        if (stoneRoad != null)
        {
            InventoryManager.Instance.AddItem(stoneRoad, 2);
        }
    }

    private void Update()
    {
        stateMachine?.Update();
    }

    private void InitializeSystems()
    {
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnUnitDead += HandleUnitDead;
            UnitManager.Instance.OnUnitSpawned += HandleUnitSpawned;
        }

        if (EconomyManager.InstanceExists)
        {
            EconomyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
        }

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += HandleNodeClick;
            InputManager.Instance.OnClickUnit += HandleUnitClick;
            InputManager.Instance.OnCancel += HandleCancel;
            InputManager.Instance.OnRightClickNode += HandleRightClick;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Use direct null checks on Instance which respects applicationIsQuitting
        var input = InputManager.Instance;
        if (input != null)
        {
            input.OnClickNode -= HandleNodeClick;
            input.OnClickUnit -= HandleUnitClick;
            input.OnCancel -= HandleCancel;
            input.OnRightClickNode -= HandleRightClick;
        }

        var unitM = UnitManager.Instance;
        if (unitM != null)
        {
            unitM.OnUnitDead -= HandleUnitDead;
            unitM.OnUnitSpawned -= HandleUnitSpawned;
        }

        var econ = EconomyManager.Instance;
        if (econ != null)
        {
            econ.OnCurrencyChanged -= HandleCurrencyChanged;
        }
    }

    private void HandleUnitDead(Unit unit)
    {
        if (unit != null)
        {
            if (unit.Data.category == UnitCategory.Core)
            {
                GameOver();
            }
            else if (!unit.IsPlayerTeam)
            {
                TotalEnemiesKilled++;
            }
        }
    }

    private void HandleNodeClick(GridNode node)
    {
        if (CurrentState is GameState gameState)
        {
            gameState.OnClickNode(node);
        }
    }

    private void HandleUnitClick(Unit unit)
    {
        if (CurrentState is GameState gameState)
        {
            gameState.OnClickUnit(unit);
        }
    }

    private void HandleRightClick(GridNode node)
    {
        HandleCancel();
    }

    private void HandleCancel()
    {
        if (CurrentState is GameState gameState)
        {
            gameState.OnCancel();
        }
    }

    public void ChangeState(BaseState<GameManager> newState)
    {
        stateMachine?.ChangeState(newState);
    }

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
        if (MetaManager.Instance != null)
            MetaManager.Instance.SetRunResult(true, CurrentWave, TotalEnemiesKilled, TotalGoldEarned);

        if (SceneController.Instance != null)
            SceneController.Instance.LoadResult();
    }

    public void GameOver()
    {
        if (MetaManager.Instance != null)
            MetaManager.Instance.SetRunResult(false, CurrentWave - 1, TotalEnemiesKilled, TotalGoldEarned);

        if (SceneController.Instance != null)
            SceneController.Instance.LoadResult();
    }

    public void SelectUnitToPlace(UnitDataSO unitData, SelectionSource source, GridNode originalNode = null, Unit originalUnit = null)
    {
        if (!IsMaintenancePhase) return;

        SelectedUnitToPlace = unitData;
        SelectedTileToPlace = null;
        CurrentSelectionSource = source;
        OriginalNode = originalNode;
        PickedUpUnit = originalUnit;
    }

    public void SelectTileToPlace(TileDataSO tileData, SelectionSource source, GridNode originalNode = null)
    {
        if (!IsMaintenancePhase) return;

        SelectedTileToPlace = tileData;
        SelectedUnitToPlace = null;
        CurrentSelectionSource = source;
        OriginalNode = originalNode;
        PickedUpUnit = null;
    }

    public void ClearSelection(bool useSafetyNet = true)
    {
        if (useSafetyNet && PickedUpUnit != null && OriginalNode != null)
        {
            UnitManager.Instance?.SpawnUnit(PickedUpUnit.Data, OriginalNode);
        }

        SelectedUnitToPlace = null;
        SelectedTileToPlace = null;
        CurrentSelectionSource = SelectionSource.Inventory;
        OriginalNode = null;
        PickedUpUnit = null;
    }

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

    private void HandleUnitSpawned(Unit unit)
    {
        if (unit != null && unit.IsPlayerTeam && unit.Data.category != UnitCategory.Core)
        {
            TotalUnitsUsed++;
        }
    }

    private void HandleCurrencyChanged(CurrencyType type, int current, int changed)
    {
        if (type == CurrencyType.Gold && changed > 0)
        {
            TotalGoldEarned += changed;
        }
    }

    public void FocusCamera(Vector3 targetPosition) => CameraManager.Instance?.FocusOn(targetPosition);
    public void FocusCamera(GridNode node) { if (node != null) FocusCamera(node.WorldPosition); }
    public void ResetCamera() => CameraManager.Instance?.ResetPosition();

}
