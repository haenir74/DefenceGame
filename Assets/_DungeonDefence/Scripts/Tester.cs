//
using UnityEngine;
using System.Collections.Generic;

public class DataTester : MonoBehaviour
{
    [Header("⚔️ Unit Test Data")]
    [Tooltip("좌클릭 시 소환할 아군 (예: 고블린, 힐러)")]
    public UnitDataSO allyUnitData;  
    
    [Tooltip("우클릭 시 소환할 적군 (예: 전사, 궁수)")]
    public UnitDataSO enemyUnitData; 

    [Tooltip("C키를 누르면 소환할 코어 유닛 (패배 조건)")]
    public UnitDataSO coreUnitData; // [추가]

    [Header("🗺️ Tile Test Data")]
    [Tooltip("T키를 누르면 마우스 위치 타일을 이걸로 변경 (예: 도로)")]
    public TileDataSO changeTileData; 

    private void Start()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnClickNode += HandleLeftClick;
            InputManager.Instance.OnRightClickNode += HandleRightClick;
            Debug.Log("<color=cyan>[Tester] Ready! (L:Ally / R:Enemy / C:Core / Space:Mana / T:Tile)</color>");
        }
    }

    private void Update()
    {
        // [Space] 마나 충전
        if (Input.GetKeyDown(KeyCode.Space)) ChargeAllMana();

        // [T] 타일 변경
        if (Input.GetKeyDown(KeyCode.T)) ChangeTileUnderMouse();
        
        // [K] 유닛 처치 (코어 파괴 테스트용)
        if (Input.GetKeyDown(KeyCode.K)) DamageAllUnits(50);

        // [C] 코어 소환 (마우스 위치)
        if (Input.GetKeyDown(KeyCode.C))
        {
            SpawnUnitUnderMouse(coreUnitData);
        }
    }

    // 마우스 위치에 특정 유닛 소환 (공용 메서드)
    private void SpawnUnitUnderMouse(UnitDataSO data)
    {
        if (data == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            GridNode node = GridManager.Instance.GetNode(hit.point);
            if (node != null)
            {
                Unit spawned = UnitManager.Instance.SpawnUnit(data, node);
                if (spawned != null) 
                    Debug.Log($"[Spawn] {spawned.name} 소환 완료!");
            }
        }
    }

    private void HandleLeftClick(GridNode node)
    {
        if (allyUnitData != null) UnitManager.Instance.SpawnUnit(allyUnitData, node);
    }

    private void HandleRightClick(GridNode node)
    {
        if (enemyUnitData != null) UnitManager.Instance.SpawnUnit(enemyUnitData, node);
    }

    // ... (ChangeTileUnderMouse, ChargeAllMana, DamageAllUnits, OnGUI는 기존과 동일) ...
    private void ChangeTileUnderMouse()
    {
        if (changeTileData == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GridNode node = GridManager.Instance.GetNode(hit.point);
            if (node != null && node.Tile != null) node.Tile.Setup(changeTileData);
        }
    }

    private void ChargeAllMana()
    {
        foreach (var unit in UnitManager.Instance.GetAllUnits())
            if (unit.Combat != null && !unit.Combat.IsDead) unit.Combat.AddMana(100f);
    }

    private void DamageAllUnits(float amount)
    {
        foreach (var unit in new List<Unit>(UnitManager.Instance.GetAllUnits()))
            if (unit.Combat != null && !unit.Combat.IsDead) unit.Combat.TakeDamage(amount);
    }

    private void OnGUI()
    {
        // ... (기존 OnGUI 코드 유지) ...
        var units = UnitManager.Instance.GetAllUnits();
        Camera cam = Camera.main;
        if (cam == null) return;

        foreach (var unit in units)
        {
            if (unit == null || unit.Combat.IsDead) continue;
            Vector3 screenPos = cam.WorldToScreenPoint(unit.transform.position + Vector3.up * 2f);
            if (screenPos.z < 0) continue;

            string info = $"{unit.Data.unitName}\n" +
                          $"<color=#ff4444>HP {unit.Combat.CurrentHp:F0}</color> / " +
                          $"<color=#4444ff>MP {unit.Combat.CurrentMana:F0}</color>";

            GUIStyle style = new GUIStyle();
            style.richText = true;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            
            GUI.color = Color.black;
            GUI.Label(new Rect(screenPos.x - 51, Screen.height - screenPos.y - 51, 100, 60), info, style);
            GUI.color = Color.white;
            GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50, 100, 60), info, style);
        }
    }
}