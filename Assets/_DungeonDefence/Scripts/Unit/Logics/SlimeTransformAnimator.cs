using UnityEngine;

/// <summary>
/// 슬라임 유닛 전용 Transform 애니메이션.
/// 모든 애니메이션은 localScale 대신 localPosition(Y축)만 사용.
///
/// ■ Idle  : Y 방향 부드러운 바운스
/// ■ Move  : Y 방향 빠른 바운스
/// ■ Attack: 위로 점프 → 빠르게 내리찍기 (스탬프)
/// ■ Death : Y를 0으로 내려 바닥에 납작해지는 연출 (Scale만 예외 허용)
/// </summary>
public class SlimeTransformAnimator : MonoBehaviour
{
    [Header("Idle 바운스")]
    [SerializeField] private float idleBounceSpeed = 2.5f;
    [SerializeField] private float idleBounceHeight = 0.05f;   // Y 오프셋 최대치

    [Header("Move 바운스")]
    [SerializeField] private float moveBounceSpeed = 5f;
    [SerializeField] private float moveBounceHeight = 0.08f;

    [Header("Attack 점프-스탬프")]
    [SerializeField] private float jumpHeight = 0.4f;          // 점프 올라가는 높이
    [SerializeField] private float jumpUpDuration = 0.12f;     // 올라가는 데 걸리는 시간
    [SerializeField] private float stampDownDuration = 0.07f;  // 내려찍히는 시간 (짧을수록 강렬)
    [SerializeField] private float recoveryDuration = 0.08f;   // 원위치 복귀 시간

    [Header("Death")]
    [SerializeField] private float deathShrinkDuration = 0.3f;

    // ─── 런타임 상태 ──────────────────────────────────────────────────────
    private Unit unit;
    private Vector3 baseScale;
    private Vector3 baseLocalPos;
    private float idleTimer;

    // 공격 점프 상태
    private enum AttackPhase { None, JumpUp, StampDown, Recovery }
    private AttackPhase attackPhase = AttackPhase.None;
    private float attackTimer;

    // 사망 상태
    private bool isShrinking;
    private float shrinkTimer;

    // ─────────────────────────────────────────────────────────────────────

    public void Initialize(Unit owner)
    {
        this.unit = owner;
        baseScale = transform.localScale;
        baseLocalPos = transform.localPosition;

        attackPhase = AttackPhase.None;
        isShrinking = false;
    }

    private void Update()
    {
        if (unit == null) return;

        // ── 사망 처리 ──
        if (isShrinking) { UpdateDeathShrink(); return; }
        if (unit.IsDead) { StartDeathShrink(); return; }

        // ── 공격 점프 ──
        if (attackPhase != AttackPhase.None) { UpdateAttackJump(); return; }

        // ── 이동 / 대기 ──
        if (unit.Movement != null && unit.Movement.IsMoving)
            AnimateMove();
        else
            AnimateIdle();
    }

    // ─── Idle: 부드러운 Y 바운스 ─────────────────────────────────────────

    private void AnimateIdle()
    {
        idleTimer += Time.deltaTime * idleBounceSpeed;
        float bounce = Mathf.Abs(Mathf.Sin(idleTimer * Mathf.PI));   // 0~1, 항상 양수
        float yOffset = bounce * idleBounceHeight;

        transform.localPosition = baseLocalPos + new Vector3(0f, yOffset, 0f);
        transform.localScale = baseScale; // 스케일 고정
    }

    // ─── Move: 빠른 Y 바운스 ─────────────────────────────────────────────

    private void AnimateMove()
    {
        idleTimer += Time.deltaTime * moveBounceSpeed;
        float bounce = Mathf.Abs(Mathf.Sin(idleTimer * Mathf.PI));
        float yOffset = bounce * moveBounceHeight;

        transform.localPosition = baseLocalPos + new Vector3(0f, yOffset, 0f);
        transform.localScale = baseScale;
    }

    // ─── Attack: 점프 → 스탬프 ───────────────────────────────────────────

    /// <summary>외부(UnitSpriteAnimator)에서 공격 시점에 호출.</summary>
    public void PlayAttackSquash()
    {
        attackPhase = AttackPhase.JumpUp;
        attackTimer = 0f;
    }

    private void UpdateAttackJump()
    {
        attackTimer += Time.deltaTime;

        Vector3 jumpPeak = baseLocalPos + new Vector3(0f, jumpHeight, 0f);

        switch (attackPhase)
        {
            case AttackPhase.JumpUp:
                {
                    float t = Mathf.Clamp01(attackTimer / jumpUpDuration);
                    // ease-out: 처음에 빠르고 정점에서 느리게
                    float eased = 1f - (1f - t) * (1f - t);
                    transform.localPosition = Vector3.Lerp(baseLocalPos, jumpPeak, eased);
                    transform.localScale = baseScale;

                    if (t >= 1f) { attackPhase = AttackPhase.StampDown; attackTimer = 0f; }
                    break;
                }
            case AttackPhase.StampDown:
                {
                    float t = Mathf.Clamp01(attackTimer / stampDownDuration);
                    // ease-in: 처음에 느리고 바닥 직전에 빠르게 (중력감)
                    float eased = t * t;
                    transform.localPosition = Vector3.Lerp(jumpPeak, baseLocalPos, eased);
                    transform.localScale = baseScale;

                    if (t >= 1f) { attackPhase = AttackPhase.Recovery; attackTimer = 0f; }
                    break;
                }
            case AttackPhase.Recovery:
                {
                    // 이미 baseLocalPos에 도달했으므로 즉시 완료
                    transform.localPosition = baseLocalPos;
                    transform.localScale = baseScale;
                    attackPhase = AttackPhase.None;
                    break;
                }
        }
    }

    // ─── Death: Y=0으로 납작해짐 ─────────────────────────────────────────

    private void StartDeathShrink()
    {
        isShrinking = true;
        shrinkTimer = 0f;
    }

    private void UpdateDeathShrink()
    {
        shrinkTimer += Time.deltaTime;
        float t = Mathf.Clamp01(shrinkTimer / deathShrinkDuration);

        // 아래로 꺼지는 느낌: Y만 줄임
        float yOffset = Mathf.Lerp(0f, -baseLocalPos.y - 0.1f, t);
        transform.localPosition = baseLocalPos + new Vector3(0f, yOffset, 0f);

        // 크기도 천천히 줄어듦 (사망 시에만 스케일 사용)
        transform.localScale = Vector3.Lerp(baseScale, Vector3.zero, t);

        if (t >= 1f) isShrinking = false;
    }

    // ─── 풀 재사용 초기화 ────────────────────────────────────────────────

    public void ResetAnimation()
    {
        attackPhase = AttackPhase.None;
        isShrinking = false;
        idleTimer = 0f;
        attackTimer = 0f;
        transform.localPosition = baseLocalPos;
        transform.localScale = baseScale;
    }
}
