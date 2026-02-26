using UnityEngine;

public class UnitMecanimAnimator : MonoBehaviour
{
    private Animator animator;
    private Unit unit;
    private float lastXPos;
    private SpriteRenderer spriteRenderer;

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        this.animator = GetComponent<Animator>();
        if (this.animator == null) this.animator = GetComponentInChildren<Animator>();
        
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        if (this.spriteRenderer == null) this.spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Awake()
    {
        // For prefabs already having it
        if (unit == null) unit = GetComponent<Unit>();
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (unit == null || animator == null) return;

        // Set IsWalking based on movement
        bool isMoving = unit.Movement != null && unit.Movement.IsMoving;
        animator.SetBool("IsWalking", isMoving);

        // Update flipping
        UpdateFlipping();
    }

    private void UpdateFlipping()
    {
        if (spriteRenderer == null) return;

        if (unit.FSM != null && unit.FSM.CurrentState is UnitCombatState)
        {
            if (UnitManager.Instance != null)
            {
                var target = UnitManager.Instance.GetOpponentAt(unit.Coordinate, unit.IsPlayerTeam);
                if (target != null && target.transform != null)
                {
                    spriteRenderer.flipX = target.transform.position.x < transform.position.x;
                }
            }
        }
        else if (unit.Movement != null && unit.Movement.IsMoving)
        {
            if (transform.position.x > lastXPos + 0.001f)
                spriteRenderer.flipX = false;
            else if (transform.position.x < lastXPos - 0.001f)
                spriteRenderer.flipX = true;
        }

        lastXPos = transform.position.x;
    }

    public void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
}
