using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private GameController controller;

    protected override void Awake()
    {
        base.Awake();
        this.controller = GetComponent<GameController>();
        
        if (this.controller == null)
        {
            this.controller = FindObjectOfType<GameController>();
        }
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