/// <summary>유닛 관련 전역 상수.</summary>
public static class UnitConstants
{
    /// <summary>
    /// 모든 유닛 루트 오브젝트의 고정 월드 Y 좌표.
    /// GridSystem은 Y=0으로 WorldPosition을 생성하므로 이 값으로 명시적으로 보정.
    /// 타일 프리팹이 Scale.y=0.2 큐브이고 피벗이 중앙이라면 타일 상단은 Y=0.1.
    /// 유닛은 그 위 0.1 여유를 두어 Y=0.2에 배치.
    /// </summary>
    public const float UNIT_HEIGHT = 0.2f;
}
