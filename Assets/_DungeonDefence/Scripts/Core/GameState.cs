using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameState : BaseState<GameManager>
{
    protected GameState(GameManager manager) : base(manager) { }

    public virtual void OnClickNode(GridNode node) { }
    public virtual void OnClickUnit(Unit unit) { }
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
    public MaintenanceState(GameManager manager) : base(manager) { }

    public override void OnEnter()
    {
        UIManager.Instance?.SwitchToMaintenancePhase();
    }

    public override void OnExit()
    {
    }

    public override void OnClickNode(GridNode node)
    {
        if (node == null) return;

        ShowNodeInfo(node);
    }

    public override void OnClickUnit(Unit unit)
    {
        if (unit == null) return;

        if (Input.GetMouseButtonDown(0) && IsDoubleClick())
        {
            
            InventoryManager.Instance?.AddItem(unit.Data, 1);
            UnitManager.Instance?.DespawnUnit(unit);
            return;
        }

        if (unit.CurrentNode != null)
        {
            ShowNodeInfo(unit.CurrentNode);
        }
    }

    private float lastClickTime = 0f;
    private bool IsDoubleClick()
    {
        float time = Time.time;
        if (time - lastClickTime < 0.3f)
        {
            lastClickTime = 0;
            return true;
        }
        lastClickTime = time;
        return false;
    }

    public override void OnCancel()
    {

        if (GameManager.Instance.PickedUpUnit != null)
        {
            GameManager.Instance.PickedUpUnit.SetVisualVisible(true);
        }
        GameManager.Instance.ClearSelection(false);
    }

    private void ShowNodeInfo(GridNode node)
    {
        var units = UnitManager.Instance.GetUnitsOnNode(node);
        string tileName = node.CurrentTileData != null ? node.CurrentTileData.Name : "Empty";
        
    }
}

public class BattleState : GameState
{
    private readonly BattleStateDTO dto;
    private bool isBattleOver = false;

    public BattleStateDTO DTO => dto;

    public BattleState(GameManager manager, BattleStateDTO dto) : base(manager)
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
            
        }
    }

    public override void OnClickUnit(Unit unit)
    {
        if (unit == null) return;
        
    }

    private void HandleWaveCompleted()
    {
        if (isBattleOver) return;
        isBattleOver = true;
        Controller.StartCoroutine(ReturnToMaintenanceCo());
    }

    private IEnumerator ReturnToMaintenanceCo()
    {
        yield return new WaitForSeconds(2.0f);
        GameManager.Instance.EndBattlePhase();
    }
}

public class GameOverState : GameState
{
    public GameOverState(GameManager manager) : base(manager) { }

    public override void OnEnter()
    {
        
        
    }

    public override void OnClickNode(GridNode node) { }
    public override void OnCancel() { }
}

public class VictoryState : GameState
{
    public VictoryState(GameManager manager) : base(manager) { }

    public override void OnEnter()
    {
        

    }

    public override void OnClickNode(GridNode node) { }
    public override void OnCancel() { }
}



