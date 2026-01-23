using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : Singleton<BuildManager>
{
    [Header("Structure Settings")]
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private int buildCost = 50;

    [Header("Unit Settings")]
    [SerializeField] private List<UnitDataSO> unitLibrary;
    private int selectedUnitIndex = 0;

    void Start()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += BuildStructure;
            InputManager.Instance.OnRightClickNode += PlaceUnit;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode -= BuildStructure;
            InputManager.Instance.OnRightClickNode -= PlaceUnit;
        }
    }
    
    public void SelectUnit(int index)
    {
        if (unitLibrary != null && index >= 0 && index < unitLibrary.Count)
        {
            selectedUnitIndex = index;
            Debug.Log($"Selected Unit: {unitLibrary[index].unitName}");
        }
    }

    private void BuildStructure(Node node)
    {
        if (node == null) return;

        if (node.TileEffect != null)
        {
            Debug.Log("이미 건물이 존재합니다.");
            return;
        }

        var gameContext = GameManager.Instance?.Context;
        if (gameContext == null) return;

        if (gameContext.playerGold < buildCost)
        {
            Debug.Log($"골드가 부족합니다. (보유: {gameContext.playerGold}, 필요: {buildCost})");
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
            Debug.LogError("생성된 프리팹에 Tile 컴포넌트가 없습니다.");
            Destroy(newBuildingObj);
            gameContext.playerGold += buildCost; 
        }
    }

    private void PlaceUnit(Node node)
    {
        if (node == null) return;
        if (unitLibrary == null || unitLibrary.Count == 0)return;
        
        UnitDataSO selectedUnitData = unitLibrary[selectedUnitIndex];

        var gameContext = GameManager.Instance?.Context;
        if (gameContext == null) return;

        if (gameContext.playerGold < selectedUnitData.cost)
        {
            Debug.Log($"골드가 부족합니다. (현재: {gameContext.playerGold}, 필요: {selectedUnitData.cost})");
            return;
        }

        if (selectedUnitData.prefab == null)
        {
            Debug.LogWarning($"Prefab is missing for unit: {selectedUnitData.unitName}");
            return;
        }

        Vector3 spawnPos = node.WorldPosition;
        var map = GameManager.Instance.Context.map;
        float cellSize = GridManager.Instance.Data.cellSize;

        if (map != null && map.SpawnNode != null)
        {
            spawnPos = GridManager.Instance.GridSystem.GetPlacementPosition(
                node, 
                map.SpawnNode,
                Team.Ally, 
                cellSize
            );
        }

        gameContext.playerGold -= selectedUnitData.cost;
        GameObject unitObj = Instantiate(selectedUnitData.prefab, spawnPos, Quaternion.identity);
        
        Unit unitScript = unitObj.GetComponent<Unit>();
        if (unitScript != null)
        {
            unitScript.InitializeUnit(selectedUnitData);
            unitScript.SetNode(node);
        }
        else
        {
            Debug.LogError("생성된 유닛 프리팹에 Unit 컴포넌트가 없습니다.");
            Destroy(unitObj);
            gameContext.playerGold += selectedUnitData.cost;
        }
    }
}
