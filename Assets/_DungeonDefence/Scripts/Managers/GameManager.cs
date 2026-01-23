using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panex.Inventory.Controller;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameContext context;
    public GameContext Context => context;


    //FSM
    private StateMachine stateMachine;
    public IGameState CurrentGameState => stateMachine.CurrentState as IGameState;

    [Header("Inventories")]
    public InventoryController structureInventory;
    public InventoryController unitInventory;

    protected override void Awake()
    {
        base.Awake();
        
        if (context == null) context = new GameContext();
        stateMachine = new StateMachine("GameManager");
    }

    void Start()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += HandleNodeClick;
            InputManager.Instance.OnRightClickNode += HandleRightClick;
        }
        ChangeState(new NormalState());
    }

    public void ChangeState(IGameState newState)
    {
        stateMachine.ChangeState(newState);
    }

    private void HandleNodeClick(Node node)
    {
        CurrentGameState?.OnClickNode(node);
    }

    private void HandleRightClick(Node node)
    {
        CurrentGameState?.OnCancel();
    }

    void Update()
    {
        stateMachine.Update();
    }

    public void ConsumeInventoryItem(BaseItemSO item)
    {        
        if (item is StructureItemSO && structureInventory != null)
        {
            structureInventory.RemoveItem(item, 1);
        }
        else if (item is UnitItemSO && unitInventory != null)
        {
            unitInventory.RemoveItem(item, 1);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        var inputMgr = InputManager.Instance;
        var gridMgr = GridManager.Instance;

        if (inputMgr != null && gridMgr != null)
        {
            inputMgr.OnHoverNodeChanged -= gridMgr.OnHoverChanged;
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0; 
    }
}