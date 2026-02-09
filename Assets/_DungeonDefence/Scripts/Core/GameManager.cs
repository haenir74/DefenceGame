using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] UnitDataSO coreUnit;

    private GameController controller;

    public UnitDataSO SelectedUnitToPlace { get; private set; }
    public int CurrentWave { get; private set; } = 1;

    protected override void Awake()
    {
        base.Awake();
        this.controller = GetComponent<GameController>();
        if (this.controller == null) this.controller = FindObjectOfType<GameController>();
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

    private void SpawnCore()
    {
        if (coreUnit == null) return;
        GridNode coreNode = GridManager.Instance.GetCoreNode();
        if (coreNode != null)
        {
            Unit core = UnitManager.Instance.SpawnUnit(coreUnit, coreNode);
            FocusCamera(coreNode);
        }
    }

    public void StartBattlePhase()
    {
        if(controller.CurrentState is MaintenanceState)
        {
            ClearSelection();
            int waveIndex = CurrentWave - 1;

            var battleDTO = new BattleStateDTO(CurrentWave);
            controller.ChangeState(new BattleState(controller, battleDTO));
            CurrentWave++;
        }
    }

    // Helper
    public void SelectUnitToPlace(UnitDataSO unitData)
    {
        if (!(controller.CurrentState is MaintenanceState)) return;
        this.SelectedUnitToPlace = unitData;
    }

    public void ClearSelection()
    {
        this.SelectedUnitToPlace = null;
    }

    public void ChangeState(BaseState<GameController> newState)
    {
        controller?.ChangeState(newState);
    }

    public void GameOver()
    {
        controller?.GameOver();
    }

    public bool IsInState<T>() where T : GameState
    {
        return controller?.CurrentState is T;
    }

    // EconomyManager
    public int Gold => EconomyManager.Instance ? EconomyManager.Instance.CurrentGold : 0;
    public void AddGold(int amount)
    {
        EconomyManager.Instance?.AddGold(amount);
    }
    public bool TrySpendGold(int amount)
    {
        return EconomyManager.Instance != null && EconomyManager.Instance.TrySpendGold(amount);
    }

    // CameraManager
    public void FocusCamera(Vector3 targetPosition)
    {
        CameraManager.Instance?.FocusOn(targetPosition);
    }
    public void FocusCamera(GridNode node)
    {
        if (node != null)
            CameraManager.Instance?.FocusOn(node.WorldPosition);
    }
    public void ResetCamera()
    {
        CameraManager.Instance?.ResetPosition();
    }
}