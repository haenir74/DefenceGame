using UnityEngine;

public class UnitSpriteAnimator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Unit unit;
    private Sprite[] frames;

    private float timer;
    [SerializeField] private float frameRate = 0.25f; // Time per frame
    private int currentFrameIndex = 0;
    private float lastXPos;

    private SlimeTransformAnimator slimeAnimator;
    private bool useSpriteSheet;

    public void Initialize(Unit unit, SpriteRenderer renderer)
    {
        this.unit = unit;
        this.spriteRenderer = renderer;

        if (unit.Data != null && unit.Data.icon != null)
        {
            string texName = unit.Data.icon.texture.name;

            // 아군/적군 경로 모두 시도
            string path = unit.Data.isPlayerTeam
                ? $"Sprites/Allies/{texName}"
                : $"Sprites/Enemies/{texName}";

            frames = Resources.LoadAll<Sprite>(path);

            // 경로로 못 찾으면 아이콘 단독 사용
            if (frames == null || frames.Length == 0)
                frames = new Sprite[] { unit.Data.icon };
        }

        // 6프레임 이상이면 스프라이트 시트 모드, 아니면 Transform 애니메이션
        useSpriteSheet = frames != null && frames.Length >= 6;

        if (!useSpriteSheet)
        {
            // 단일 스프라이트: 아이콘을 바로 표시
            if (spriteRenderer != null && unit.Data != null && unit.Data.icon != null)
                spriteRenderer.sprite = unit.Data.icon;

            // SlimeTransformAnimator가 이미 붙어있으면 재사용, 없으면 추가
            slimeAnimator = GetComponentInChildren<SlimeTransformAnimator>();
            if (slimeAnimator == null)
            {
                // 프리팹에 없으면 spriteRenderer가 있는 오브젝트에 추가
                GameObject targetObj = spriteRenderer != null ? spriteRenderer.gameObject : gameObject;
                slimeAnimator = targetObj.AddComponent<SlimeTransformAnimator>();
            }
            slimeAnimator.Initialize(unit);
        }
    }

    private void Update()
    {
        if (unit == null || unit.IsDead) return;

        // Transform 애니메이션 모드는 SlimeTransformAnimator가 알아서 처리
        if (!useSpriteSheet)
        {
            UpdateFlipping();
            return;
        }

        // ─── 스프라이트 시트 모드 (6프레임 이상) ──────────────────────
        if (frames == null || frames.Length < 6) return;

        int baseFrame = 0; // Idle

        if (unit.FSM != null && unit.FSM.CurrentState is UnitCombatState)
        {
            baseFrame = 2; // Attack
        }
        else if (unit.Movement != null && unit.Movement.IsMoving)
        {
            baseFrame = 4; // Move
        }

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % 2;
            if (spriteRenderer != null)
                spriteRenderer.sprite = frames[baseFrame + currentFrameIndex];
        }

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

    /// <summary>공격 시점에 호출. SlimeTransformAnimator가 있으면 squash 애니메이션 재생.</summary>
    public void TriggerAttackAnimation()
    {
        if (slimeAnimator != null)
            slimeAnimator.PlayAttackSquash();
    }
}
