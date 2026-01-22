using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameContext context;
    public GameContext Context => context;

    void Awake()
    {
        Instance = this;
        context = new GameContext();
    }

    void Start()
    {
        if (InputManager.Instance != null && GridManager.Instance != null)
        {
            InputManager.Instance.OnHoverNodeChanged += GridManager.Instance.OnHoverChanged;
            
            // 나중에 건설 매니저가 생기면?
            // InputManager.Instance.OnClickNode += BuildManager.Instance.HandleClick;
        }
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null && GridManager.Instance != null)
        {
            InputManager.Instance.OnHoverNodeChanged -= GridManager.Instance.OnHoverChanged;
        }
    }
}