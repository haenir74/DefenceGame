using UnityEngine;

/// <summary>
/// 슬라임 유닛 전용 Transform 애니메이션.
/// 스프라이트 시트 없이 Scale/Position만으로 Idle 탄성, 공격 찌그러짐, 이동 늘어남을 표현한다.
/// UnitPrefab의 자식 "Model" 오브젝트에 부착.
/// </summary>
public class SlimeTransformAnimator : MonoBehaviour
{
    [Header("Idle Animation")]
    [SerializeField] private float idleBounceSpeed = 2.5f;
    [SerializeField] private float idleBounceAmplitude = 0.08f;

    [Header("Attack Animation")]
    [SerializeField] private float attackSquashDuration = 0.15f;
    [SerializeField] private Vector3 attackSquashScale = new Vector3(1.3f, 0.7f, 1f);

    [Header("Move Animation")]
    [SerializeField] private float moveStretchX = 1.15f;
    [SerializeField] private float moveStretchY = 0.88f;

    [Header("Death Animation")]
    [SerializeField] private float deathShrinkDuration = 0.3f;

    // ─── 런타임 ──────────────────────────────────────────────────────
    private Unit unit;
    private Vector3 baseScale;
    private Vector3 baseLocalPos;
    private float idleTimer;

    // Attack squash state
    private bool isSquashing;
    private float squashTimer;

    // Death shrink state
    private bool isShrinking;
    private float shrinkTimer;

    public void Initialize(Unit owner)
    {
        this.unit = owner;
        baseScale = transform.localScale;
        baseLocalPos = transform.localPosition;
    }

    private void Update()
    {
        if (unit == null) return;

        if (isShrinking)
        {
            UpdateDeathShrink();
            return;
        }

        if (unit.IsDead)
        {
            StartDeathShrink();
            return;
        }

        if (isSquashing)
        {
            UpdateAttackSquash();
            return;
        }

        // 상태별 애니메이션
        if (unit.FSM != null && unit.FSM.CurrentState is UnitCombatState)
        {
            // 전투 중: 공격 타이밍에 squash
            AnimateIdle(); // 전투 대기 중에도 살짝 탄성
        }
        else if (unit.Movement != null && unit.Movement.IsMoving)
        {
            AnimateMove();
        }
        else
        {
            AnimateIdle();
        }
    }

    // ─── Idle: 위아래 탄성 바운스 ─────────────────────────────────────

    private void AnimateIdle()
    {
        idleTimer += Time.deltaTime * idleBounceSpeed;
        float bounce = Mathf.Sin(idleTimer * Mathf.PI * 2f);

        float scaleX = baseScale.x + bounce * idleBounceAmplitude * 0.5f;
        float scaleY = baseScale.y - bounce * idleBounceAmplitude;
        transform.localScale = new Vector3(scaleX, scaleY, baseScale.z);

        float yOffset = Mathf.Abs(bounce) * idleBounceAmplitude * 0.3f;
        transform.localPosition = baseLocalPos + new Vector3(0f, yOffset, 0f);
    }

    // ─── Move: 이동 방향으로 늘어남 ───────────────────────────────────

    private void AnimateMove()
    {
        idleTimer += Time.deltaTime * idleBounceSpeed * 1.5f;
        float bounce = Mathf.Sin(idleTimer * Mathf.PI * 2f);

        float sx = baseScale.x * moveStretchX + bounce * 0.03f;
        float sy = baseScale.y * moveStretchY - bounce * 0.02f;
        transform.localScale = new Vector3(sx, sy, baseScale.z);

        float yOffset = Mathf.Abs(bounce) * 0.05f;
        transform.localPosition = baseLocalPos + new Vector3(0f, yOffset, 0f);
    }

    // ─── Attack: 찌그러짐 후 복원 ────────────────────────────────────

    /// <summary>외부에서 공격 시점에 호출하여 squash 애니메이션 재생.</summary>
    public void PlayAttackSquash()
    {
        isSquashing = true;
        squashTimer = 0f;
    }

    private void UpdateAttackSquash()
    {
        squashTimer += Time.deltaTime;
        float t = squashTimer / attackSquashDuration;

        if (t < 0.5f)
        {
            // 찌그러지는 단계
            float p = t * 2f;
            transform.localScale = Vector3.Lerp(baseScale, attackSquashScale, p);
        }
        else if (t < 1f)
        {
            // 복원 단계
            float p = (t - 0.5f) * 2f;
            transform.localScale = Vector3.Lerp(attackSquashScale, baseScale, p);
        }
        else
        {
            transform.localScale = baseScale;
            isSquashing = false;
        }
    }

    // ─── Death: 바닥으로 납작해지며 사라짐 ────────────────────────────

    private void StartDeathShrink()
    {
        isShrinking = true;
        shrinkTimer = 0f;
    }

    private void UpdateDeathShrink()
    {
        shrinkTimer += Time.deltaTime;
        float t = Mathf.Clamp01(shrinkTimer / deathShrinkDuration);

        float sy = Mathf.Lerp(baseScale.y, 0f, t);
        float sx = Mathf.Lerp(baseScale.x, baseScale.x * 1.5f, t);
        transform.localScale = new Vector3(sx, sy, baseScale.z);
        transform.localPosition = baseLocalPos;

        if (t >= 1f)
        {
            isShrinking = false;
        }
    }

    /// <summary>풀에서 재사용 시 상태 초기화.</summary>
    public void ResetAnimation()
    {
        isSquashing = false;
        isShrinking = false;
        idleTimer = 0f;
        transform.localScale = baseScale;
        transform.localPosition = baseLocalPos;
    }
}
