using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    [Header("Structure Settings")]
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private int buildCost = 50;

    [Header("Unit Settings")]
    [SerializeField] private GameObject allyUnitPrefab;
    [SerializeField] private int unitCost = 30;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += BuildStructure;
            InputManager.Instance.OnRightClickNode += PlaceUnit;
        }
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode -= BuildStructure;
            InputManager.Instance.OnRightClickNode -= PlaceUnit;
        }
    }

    private void BuildStructure(Node node)
    {
        if (!node.IsEmpty)
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
        GameObject newBuildingObj = Instantiate(buildingPrefab, node.WorldPosition, Quaternion.identity);
        Tile newTileLogic = newBuildingObj.GetComponent<Tile>();
        if (newTileLogic != null)
        {
            node.TileEffect = newTileLogic;
        }
        else
        {
            Destroy(newBuildingObj);
            gameContext.playerGold += buildCost; 
        }
    }

    private void PlaceUnit(Node node)
    {
        // 유닛은 같은 칸에 여러 명 있을 수 있다고 가정할지, 하나만 둘지 결정 필요.
        // 현재는 간단히 제한 없음 (하지만 너무 겹치면 보기 안좋으므로 추후 수정)
        
        var gameContext = GameManager.Instance.Context;
        if (gameContext.playerGold < unitCost)
        {
            Debug.Log($"골드가 부족합니다. (현재: {gameContext.playerGold}, 필요: {unitCost})");
            return;
        }

        if (allyUnitPrefab == null)
        {
            Debug.LogWarning("Ally Unit Prefab is not assigned in BuildManager!");
            return;
        }

        gameContext.playerGold -= unitCost;
        GameObject unitObj = Instantiate(allyUnitPrefab, node.WorldPosition, Quaternion.identity);
        
        // AllyUnit 스크립트가 있다면 초기화 등 수행
        AllyUnit unitScript = unitObj.GetComponent<AllyUnit>();
        if (unitScript != null)
        {
            unitScript.SetNode(node);
        }
    }
}
