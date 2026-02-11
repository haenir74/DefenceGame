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

        if (GameManager.Instance.SelectedUnitToPlace != null)
        {
            PlaceUnit(node, GameManager.Instance.SelectedUnitToPlace);
        }
        else if (GameManager.Instance.SelectedTileToPlace != null)
        {
            PlaceTile(node, GameManager.Instance.SelectedTileToPlace);
        }
        else
        {
            ShowNodeInfo(node);
        }
    }

    public override void OnRightClickNode(GridNode node)
    {
        if(GameManager.Instance.SelectedUnitToPlace != null)
        {
            GameManager.Instance.ClearSelection();
        }
    }

    private void PlaceUnit(GridNode node, UnitDataSO unitData)
    {
        if (InventoryManager.Instance.TryConsumeItem(unitData))
        {
            UnitManager.Instance.SpawnUnit(unitData, node);
            Debug.Log($"[Unit] 배치 완료: {unitData.Name}");
        }
        else
        {
            Debug.LogWarning("인벤토리에 유닛이 부족합니다.");
            GameManager.Instance.ClearSelection();
        }
    }

    private void PlaceTile(GridNode node, TileDataSO tileData)
    {
        if (node.Tile != null && node.Tile.Data == tileData) return;
        if (node == GridManager.Instance.GetCoreNode() || node == GridManager.Instance.GetSpawnNode())
        {
            Debug.LogWarning("시작점과 코어 타일은 교체할 수 없습니다.");
            return;
        }

        if (InventoryManager.Instance.TryConsumeItem(tileData))
        {
            GridManager.Instance.ChangeTile(node, tileData);
            Debug.Log($"[Tile] 교체 완료: ({node.X}, {node.Y}) -> {tileData.Name}");
        }
        else
        {
            Debug.LogWarning("인벤토리에 타일이 부족합니다.");
            GameManager.Instance.ClearSelection();
        }
    }

    private void ShowNodeInfo(GridNode node)
    {
        var units = UnitManager.Instance.GetUnitsOnNode(node);
        string tileName = node.Tile != null ? node.Tile.Data.Name : "Empty";
        Debug.Log($"[Info] 타일: {tileName} | 유닛 수: {units.Count}");
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
        Controller.StartCoroutine(ReturnToMaintenanceCo());
    }

    private IEnumerator ReturnToMaintenanceCo()
    {
        yield return new WaitForSeconds(2.0f);
        GameManager.Instance.EndBattlePhase();
    }
}