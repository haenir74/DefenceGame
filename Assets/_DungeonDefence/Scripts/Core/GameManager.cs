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

    public void EndBattlePhase()
    {
        // 추후 보상 화면 만든 뒤 추가
        EconomyManager.Instance?.AddGold(100);
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
    public int Gold => EconomyManager.Instance ? EconomyManager.Instance.CurrentGold : 0;
    
    public void AddGold(int amount) => EconomyManager.Instance?.AddGold(amount);
    
    public bool TrySpendGold(int amount) => EconomyManager.Instance != null && EconomyManager.Instance.TrySpendGold(amount);
    // ***

    // ** CameraManager **
    public void FocusCamera(Vector3 targetPosition) => CameraManager.Instance?.FocusOn(targetPosition);
    
    public void FocusCamera(GridNode node)
    {
        if (node != null) FocusCamera(node.WorldPosition);
    }
    
    public void ResetCamera() => CameraManager.Instance?.ResetPosition();
    // ***
}