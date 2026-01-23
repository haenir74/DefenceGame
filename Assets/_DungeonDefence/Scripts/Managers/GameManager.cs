using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameContext context;
    public GameContext Context => context;

    protected override void Awake()
    {
        base.Awake();
        
        if (context == null)
        {
            context = new GameContext();
        }
    }

    void Start()
    {
        if (InputManager.Instance != null && GridManager.Instance != null)
        {
            InputManager.Instance.OnHoverNodeChanged += GridManager.Instance.OnHoverChanged;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        var inputMgr = InputManager.Instance;
        var gridMgr = GridManager.Instance;

        // 둘 중 하나라도 없으면 해제할 필요(또는 수단)가 없음
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