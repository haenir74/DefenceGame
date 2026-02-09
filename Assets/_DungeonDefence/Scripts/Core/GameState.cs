using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameState : BaseState<GameController>
{
    protected GameState(GameController controller) : base(controller) {}

    public virtual void OnClickNode(GridNode node) { }
    public virtual void OnRightClickNode(GridNode node) { }
    public virtual void OnCancel() { }
}

public class BattleStateDTO
{
    public int WaveIndex;

    public BattleStateDTO(int waveIndex)
    {
        this.WaveIndex = waveIndex;
    }
}

public class MaintenanceState : GameState
{
    public MaintenanceState(GameController controller) : base(controller) {}
    
    public override void OnEnter() 
    { 
        UIManager.Instance?.SwitchToMaintenancePhase();
    }

    public override void OnClickNode(GridNode node) 
    {
        if (node == null) return;
        var unitToPlace = GameManager.Instance.SelectedUnitToPlace;
        if (unitToPlace != null)
        {
            UnitManager.Instance.SpawnUnit(unitToPlace, node);
            GameManager.Instance.ClearSelection();
        }
        else
        {
            var units = UnitManager.Instance.GetAllUnits();
            Debug.Log($"유닛 선택 {units}");
        }
    }

    public override void OnRightClickNode(GridNode node)
    {
        if(GameManager.Instance.SelectedUnitToPlace != null)
        {
            GameManager.Instance.ClearSelection();
        }
    }
}

public class BattleState : GameState
{
    private readonly BattleStateDTO dto;
    private bool isBattleOver = false;

    public BattleStateDTO DTO => dto;

    public BattleState(GameController controller, BattleStateDTO dto) : base(controller)
    {
        this.dto = dto;
    }

    public override void OnEnter()
    {
        UIManager.Instance?.SwitchToBattlePhase();
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveCompleted += HandleWaveCompleted;
            WaveManager.Instance.StartWave(dto.WaveIndex);
        }
        else
        {
            HandleWaveCompleted();
        }
    }

    public override void OnUpdate()
    {
        if (isBattleOver) return;
    }

    public override void OnExit()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveCompleted -= HandleWaveCompleted;
    }

    public override void OnClickNode(GridNode node) 
    {
        if (node == null) return;
        var units = UnitManager.Instance.GetUnitsOnNode(node);
        if (units.Count > 0)
        {
            Debug.Log($"유닛 정보 확인: {units[0].name} (HP: {units[0].Combat.CurrentHp})");
        }
    }

    private void HandleWaveCompleted()
    {
        if (isBattleOver) return;
        isBattleOver = true;
        Controller.StartCoroutine(TransitionToMaintenance());
    }

    private IEnumerator TransitionToMaintenance()
    {
        // TODO: 보상 씬 만든 후 그쪽으로 이동
        yield return new WaitForSeconds(2.0f);
        Controller.ChangeState(new MaintenanceState(Controller));
    }
}