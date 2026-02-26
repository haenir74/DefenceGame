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

        // [REFINED] Click-to-Place removed. Clicking only shows info.
        ShowNodeInfo(node);
    }

    public override void OnClickUnit(Unit unit)
    {
        if (unit == null) return;

        // [FIX] Double Click Support for Recall (Keeping this as it's a useful shortcut)
        if (Input.GetMouseButtonDown(0) && IsDoubleClick())
        {
            Debug.Log($"[Recall] {unit.Data.Name} double-clicked. Returning to inventory.");
            InventoryManager.Instance?.AddItem(unit.Data, 1);
            UnitManager.Instance?.DespawnUnit(unit);
            return;
        }

        // [REFINED] Picking up via simple click removed. Only Drag-and-Drop is supported.
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
        // [FIX] PlacementManager가 없을 때를 대비한 안전 장치
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
        Debug.Log($"[Info] 타일: {tileName} | 유닛 수: {units.Count}");
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
            Debug.Log($"타일 클릭 - 유닛 정보 확인: {units[0].name} (HP: {units[0].Combat.CurrentHp})");
        }
    }

    public override void OnClickUnit(Unit unit)
    {
        if (unit == null) return;
        Debug.Log($"유닛 직접 클릭 - 정보 확인: {unit.name} (HP: {unit.Combat.CurrentHp})");
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
        Debug.Log("<color=red>[GameOverState] *** 게임 오버 *** 코어가 파괴되었습니다!</color>");
        // TODO: GameOverScene으로 씬 전환
        // SceneManager.LoadScene("GameOverScene");
    }

    public override void OnClickNode(GridNode node) { }
    public override void OnCancel() { }
}

public class VictoryState : GameState
{
    public VictoryState(GameManager manager) : base(manager) { }

    public override void OnEnter()
    {
        Debug.Log("<color=yellow>[VictoryState] *** 게임 클리어! *** 30웨이브를 모두 클리어했습니다!</color>");
        // TODO: VictoryScene으로 씬 전환
        // SceneManager.LoadScene("VictoryScene");
    }

    public override void OnClickNode(GridNode node) { }
    public override void OnCancel() { }
}
