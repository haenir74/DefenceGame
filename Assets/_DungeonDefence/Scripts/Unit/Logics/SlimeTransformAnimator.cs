using UnityEngine;

public class SlimeTransformAnimator : MonoBehaviour
{

    [SerializeField] private float idleBounceSpeed = 2.5f;
    [SerializeField] private float idleBounceHeight = 0.05f;

    [SerializeField] private float moveBounceSpeed = 5f;
    [SerializeField] private float moveBounceHeight = 0.08f;

    [SerializeField] private float jumpHeight = 0.4f;
    [SerializeField] private float jumpUpDuration = 0.12f;
    [SerializeField] private float stampDownDuration = 0.07f;

    [SerializeField] private float deathShrinkDuration = 0.3f;

    private Unit unit;
    private Vector3 baseScale;
    private Vector3 baseLocalPos;
    private float idleTimer;

    private enum AttackPhase { None, JumpUp, StampDown, Recovery }
    private AttackPhase attackPhase = AttackPhase.None;
    private float attackTimer;

    private bool isShrinking;
    private float shrinkTimer;

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

        if (isShrinking)
        {
            UpdateDeathAnimation();
            return;
        }

        if (attackPhase != AttackPhase.None)
        {
            UpdateAttackAnimation();
        }
        else if (unit.Movement != null && unit.Movement.IsMoving)
        {
            UpdateMoveBounce();
        }
        else
        {
            UpdateIdleBounce();
        }
    }

    private void UpdateIdleBounce()
    {
        idleTimer += Time.deltaTime * idleBounceSpeed;
        float offset = Mathf.Sin(idleTimer) * idleBounceHeight;
        transform.localScale = baseScale + new Vector3(offset, offset, offset);
    }

    private void UpdateMoveBounce()
    {
        idleTimer += Time.deltaTime * moveBounceSpeed;
        float offset = Mathf.Sin(idleTimer) * moveBounceHeight;
        transform.localScale = baseScale + new Vector3(offset, offset, offset);
    }

    public void TriggerAttackAnimation()
    {
        if (attackPhase != AttackPhase.None) return;
        attackPhase = AttackPhase.JumpUp;
        attackTimer = 0f;
    }

    public void PlayAttackSquash()
    {
        TriggerAttackAnimation();
    }

    private void UpdateAttackAnimation()
    {
        attackTimer += Time.deltaTime;

        switch (attackPhase)
        {
            case AttackPhase.JumpUp:
                float upT = attackTimer / jumpUpDuration;
                if (upT >= 1f)
                {
                    attackPhase = AttackPhase.StampDown;
                    attackTimer = 0f;
                }
                else
                {
                    float h = jumpHeight * Mathf.Sin(upT * Mathf.PI);
                    transform.localPosition = baseLocalPos + Vector3.up * h;
                }
                break;

            case AttackPhase.StampDown:
                float downT = attackTimer / stampDownDuration;
                if (downT >= 1f)
                {
                    transform.localPosition = baseLocalPos;
                    attackPhase = AttackPhase.Recovery;
                    attackTimer = 0f;
                }
                else
                {
                    float h = jumpHeight * (1f - downT);
                    transform.localPosition = baseLocalPos + Vector3.up * h;
                }
                break;

            case AttackPhase.Recovery:
                if (attackTimer > 0.1f)
                {
                    attackPhase = AttackPhase.None;
                }
                break;
        }
    }

    public void TriggerDeathAnimation()
    {
        isShrinking = true;
        shrinkTimer = 0f;
    }

    private void UpdateDeathAnimation()
    {
        shrinkTimer += Time.deltaTime;
        float t = shrinkTimer / deathShrinkDuration;
        if (t >= 1f)
        {
            transform.localScale = Vector3.zero;
        }
        else
        {
            transform.localScale = Vector3.Lerp(baseScale, Vector3.zero, t);
        }
    }
}

