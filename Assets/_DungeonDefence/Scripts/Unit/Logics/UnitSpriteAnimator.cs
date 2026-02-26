using UnityEngine;

public class UnitSpriteAnimator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Unit unit;
    private Sprite[] frames;

    private float timer;
    [SerializeField] private float frameRate = 0.25f; 
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

            
            string path = unit.Data.isPlayerTeam
                ? $"Sprites/Allies/{texName}"
                : $"Sprites/Enemies/{texName}";

            frames = Resources.LoadAll<Sprite>(path);

            
            if (frames == null || frames.Length == 0)
                frames = new Sprite[] { unit.Data.icon };
        }

        
        useSpriteSheet = frames != null && frames.Length >= 6;

        if (!useSpriteSheet)
        {
            
            if (spriteRenderer != null && unit.Data != null && unit.Data.icon != null)
                spriteRenderer.sprite = unit.Data.icon;

            
            slimeAnimator = GetComponentInChildren<SlimeTransformAnimator>();
            if (slimeAnimator == null)
            {
                
                GameObject targetObj = spriteRenderer != null ? spriteRenderer.gameObject : gameObject;
                slimeAnimator = targetObj.AddComponent<SlimeTransformAnimator>();
            }
            slimeAnimator.Initialize(unit);
        }
    }

    private void Update()
    {
        if (unit == null || unit.IsDead) return;

        
        if (!useSpriteSheet)
        {
            UpdateFlipping();
            return;
        }

        
        if (frames == null || frames.Length < 6) return;

        int baseFrame = 0; 

        if (unit.FSM != null && unit.FSM.CurrentState is UnitCombatState)
        {
            baseFrame = 2; 
        }
        else if (unit.Movement != null && unit.Movement.IsMoving)
        {
            baseFrame = 4; 
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

    
    public void TriggerAttackAnimation()
    {
        if (slimeAnimator != null)
            slimeAnimator.PlayAttackSquash();
    }
}



