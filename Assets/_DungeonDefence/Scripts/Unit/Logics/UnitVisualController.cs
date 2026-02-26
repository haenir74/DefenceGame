using UnityEngine;

/// <summary>
/// 쿼터뷰 환경에서 유닛 스프라이트를 타일과 평행하게(XZ 평면) 표시하는 컴포넌트.
///
/// ■ 회전: X축 +90° 회전을 LateUpdate()에서 매 프레임 강제 유지.
///         Mecanim Animator가 localRotation을 초기화해도 즉시 복원됨.
///
/// ■ 높이: 유닛 ROOT를 UnitConstants.UNIT_HEIGHT(=0.2)로 UnitManager/Unit.Initialize에서 설정하므로
///         여기서는 추가 Y 오프셋 없음. visualRoot.localPosition.y = 0으로 초기화.
///
/// ■ 크기: patch_unit_scales.py로 prefab localScale을 미리 패치 완료.
///         런타임 스케일 변경 없음.
/// </summary>
public class UnitVisualController : MonoBehaviour
{
    private Transform visualRoot;
    private bool isReady = false;

    /// <summary>Unit.Initialize()에서 애니메이터 초기화 전에 호출.</summary>
    public void Apply()
    {
        visualRoot = FindVisualRoot();
        if (visualRoot == null)
        {
            Debug.LogWarning($"[UnitVisualController] {name}: visualRoot를 찾지 못했습니다.");
            return;
        }

        // 회전: XZ 평면과 평행 (X축 +90°)
        visualRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // 높이: 루트 Y가 이미 UNIT_HEIGHT(0.2)이므로 localY는 0으로 초기화
        Vector3 pos = visualRoot.localPosition;
        pos.y = 0f;
        visualRoot.localPosition = pos;

        isReady = true;
    }

    /// <summary>
    /// Mecanim Animator의 localRotation override를 매 프레임 덮어씀.
    /// (Animator.Update는 Update 직후, LateUpdate 직전에 실행됨)
    /// </summary>
    private void LateUpdate()
    {
        if (!isReady || visualRoot == null) return;
        visualRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private Transform FindVisualRoot()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponentInChildren<SpriteRenderer>(true) != null)
                return child;
        }
        return null;
    }
}
