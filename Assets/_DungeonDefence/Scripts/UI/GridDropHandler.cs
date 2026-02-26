using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 그리드 타일 위에 놓인 투명 UI 오버레이에 붙여 사용.
/// 인벤토리 유닛을 드래그 앤 드롭으로 배치하는 기능을 담당.
/// GridView의 각 타일 오브젝트에 추가하거나,
/// 전체 그리드 위를 덮는 fullscreen RaycastTarget에 추가한다.
/// 
/// 실제 배치 판단은 MaintenanceState.PlaceUnit()에 위임.
/// </summary>
public class GridDropHandler : UniversalDropHandler
{
    [Tooltip("이 오브젝트가 대응하는 그리드 노드 (GridView에서 세팅)")]
    public GridNode TargetNode;

    // OnDrop is inherited from UniversalDropHandler
}
