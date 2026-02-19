using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] UnitDataSO coreUnit;

    private GameController controller;

    public int CurrentWave { get; private set; } = 1;
    public UnitDataSO SelectedUnitToPlace { get; private set; }
    public TileDataSO SelectedTileToPlace { get; private set; }
    public bool IsMaintenancePhase => controller != null && controller.CurrentState is MaintenanceState;

    protected override void Awake()
    {
        base.Awake();
        if (!TryGetComponent(out controller))
        {
            controller = FindObjectOfType<GameController>();
        }
    }

    private IEnumerator Start()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += OnGridNodeClicked;
        }
        yield return null;
        while (GridManager.Instance == null || GridManager.Instance.GetCoreNode() == null)
        {
            yield return null;
        }
        SpawnCore();
        
        if (controller != null)
        {
            controller.ChangeState(new MaintenanceState(controller));
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode -= OnGridNodeClicked;
        }
    }

    // ** Game Flow **
    public void StartBattlePhase()
    {
        if (IsMaintenancePhase)
        {
            ClearSelection();
            int waveIndex = CurrentWave - 1;
            var battleDTO = new BattleStateDTO(waveIndex);
            controller.ChangeState(new BattleState(controller, battleDTO));
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
        controller.ChangeState(new MaintenanceState(controller));
    }

    public void GameOver()
    {
        controller?.GameOver();
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
    // ***

    // ** Helper **
    private void SpawnCore()
    {
        if (coreUnit == null) return;
        GridNode coreNode = GridManager.Instance.GetCoreNode();
        if (coreNode != null)
        {
            UnitManager.Instance.SpawnUnit(coreUnit, coreNode);
            FocusCamera(coreNode);
        }
    }

    public void ChangeState(BaseState<GameController> newState)
    {
        controller?.ChangeState(newState);
    }

    public bool IsInState<T>() where T : GameState
    {
        return controller?.CurrentState is T;
    }
    // Helper

    // ** EconomyManager **
    public int GetResourceAmount(CurrencyType type)
    {
        return EconomyManager.Instance.GetCurrencyAmount(type);
    }
    public int GetCurrentGold()
    {
        return GetResourceAmount(CurrencyType.Gold);
    }

    public void AddResource(CurrencyType type, int amount)
    {
        EconomyManager.Instance.AddCurrency(type, amount);
    }
    public void AddGold(int amount)
    {
        AddResource(CurrencyType.Gold, amount);
    }

    public bool CanAfford(List<ResourceCost> costs)
    {
        return EconomyManager.Instance.CanAfford(costs);
    }

    public bool CanAfford(int amount)
    {
        var cost = new ResourceCost { type = CurrencyType.Gold, amount = amount };
        return CanAfford(new List<ResourceCost> { cost });
    }

    public bool TrySpend(List<ResourceCost> costs)
    {
        return EconomyManager.Instance.TrySpend(costs);
    }

    public bool TrySpend(CurrencyType type, int amount)
    {
        var cost = new ResourceCost { type = type, amount = amount };
        return TrySpend(new List<ResourceCost> { cost });
    }

    public bool TrySpend(int amount)
    {
        var cost = new ResourceCost { type = CurrencyType.Gold, amount = amount };
        return TrySpend(new List<ResourceCost> { cost });
    }
    // ***

    // ** CameraManager **
    public void FocusCamera(Vector3 targetPosition) => CameraManager.Instance?.FocusOn(targetPosition);
    
    public void FocusCamera(GridNode node)
    {
        if (node != null) FocusCamera(node.WorldPosition);
    }
    
    public void ResetCamera() => CameraManager.Instance?.ResetPosition();
    // ***

    // ** Interaction Logic **
    public void OnGridNodeClicked(GridNode node)
    {
        if (!IsMaintenancePhase || node == null) return;

        if (SelectedUnitToPlace != null)
        {
            if (node.CanPlaceUnit)
            {
                bool success = UnitManager.Instance.SpawnUnit(SelectedUnitToPlace, node);
                
                if (success)
                {
                    Debug.Log($"[GameManager] {SelectedUnitToPlace.Name} 배치 완료.");
                    InventoryManager.Instance.TryConsumeItem(SelectedUnitToPlace, 1);
                }
            }
            else
            {
                Debug.LogWarning("이 타일에는 유닛을 배치할 수 없습니다. (장애물 또는 이미 있음)");
            }
        }
        else if (SelectedTileToPlace != null)
        {
            if (node.UnitObject != null)
            {
                Debug.LogWarning("유닛이 있는 곳의 타일은 바꿀 수 없습니다.");
                return;
            }

            if (node.CurrentTileData != null && node.CurrentTileData.ID == SelectedTileToPlace.ID)
            {
                return;
            }

            if (node.CurrentTileData != null && !node.CurrentTileData.IsDefaultTile)
            {
                Debug.Log($"[GameManager] 기존 타일({node.CurrentTileData.Name}) 회수.");
                InventoryManager.Instance.AddItem(node.CurrentTileData, 1);
            }

            GridManager.Instance.ChangeTile(node, SelectedTileToPlace);
            InventoryManager.Instance.TryConsumeItem(SelectedTileToPlace, 1);
            ClearSelection();
        }
    }
    // ***
}