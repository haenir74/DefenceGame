using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private int buildCost = 50;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += BuildStructure;
        }
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode -= BuildStructure;
        }
    }

    private void BuildStructure(Node node)
    {
        if (node.Structure != null)
        {
            Debug.Log("이미 건물이 있습니다.");
            return;
        }

        var gameContext = GameManager.Instance.Context;
        if (gameContext.playerGold < buildCost)
        {
            Debug.Log($"골드가 부족합니다. (현재: {gameContext.playerGold}, 필요: {buildCost})");
            return;
        }

        gameContext.playerGold -= buildCost;
        GameObject newBuilding = Instantiate(buildingPrefab, node.WorldPosition, Quaternion.identity);
        node.Structure = newBuilding;

        Debug.Log($"건설 완료! 남은 골드: {gameContext.playerGold}");
    }
}