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

    public void Initialize(Unit unit, SpriteRenderer renderer)
    {
        this.unit = unit;
        this.spriteRenderer = renderer;

        if (unit.Data != null && unit.Data.icon != null)
        {
            string texName = unit.Data.icon.texture.name;
            // Load all sliced sprites from the texture in Resources
            frames = Resources.LoadAll<Sprite>($"Sprites/Enemies/{texName}");

            if (frames == null || frames.Length == 0)
            {
                // Fallback to the single icon if slicing wasn't found or it's not in Resources
                frames = new Sprite[] { unit.Data.icon };
            }
        }
    }

    private void Update()
    {
        if (unit == null || unit.IsDead || frames == null || frames.Length < 6)
            return;

        // Determine current state
        int baseFrame = 0; // Idle

        if (unit.FSM != null && unit.FSM.CurrentState is UnitCombatState)
        {
            baseFrame = 2; // Attack
        }
        else if (unit.Movement != null && unit.Movement.IsMoving)
        {
            baseFrame = 4; // Move
        }

        // Handle animation timing
        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % 2; // Each animation has 2 frames
            if (spriteRenderer != null)
                spriteRenderer.sprite = frames[baseFrame + currentFrameIndex];
        }

        // Handle sprite flipping
        if (spriteRenderer != null)
        {
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
        }

        lastXPos = transform.position.x;
    }
}
