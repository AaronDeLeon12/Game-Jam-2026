using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Walk-cycle animation for the final boss (the oruga). Uses the four clean
/// pre-cropped frames in Resources/Enemies/oruga_walk_1..4.
///
/// The sprite is rendered on a CHILD object ("BossVisual") that gets scaled,
/// so the root (with the Collider2D / Rigidbody2D / FinalBoss) keeps scale 1
/// and its collider stays the correct size. Bottom-center pivot keeps the
/// feet grounded while the body arches up and down as it crawls. Cycles
/// while FinalBoss.IsMoving; faces via FinalBoss.FacingDirection.
/// </summary>
public class FinalBossAnimator : MonoBehaviour
{
    [SerializeField] private string resourcePrefix = "Enemies/oruga_walk_";
    [SerializeField] private int frameCount = 4;
    [SerializeField] private string attackResourcePrefix = "Enemies/oruga_attack_";
    [SerializeField] private int attackFrameCount = 3;
    [SerializeField] private float framesPerSecond = 6f;
    [SerializeField] private float targetWorldHeight = 3.2f;
    [Tooltip("Visual offset from the root so the sprite sits on the collider's base.")]
    [SerializeField] private float visualYOffset = -1.1f;

    private readonly List<Sprite> frames = new List<Sprite>();
    private readonly List<Sprite> attackFrames = new List<Sprite>();
    private SpriteRenderer spriteRenderer;
    private FinalBoss boss;
    private float frameTimer;
    private int frameIndex;

    private void Awake()
    {
        boss = GetComponent<FinalBoss>();

        for (int i = 1; i <= frameCount; i++)
        {
            Sprite s = Resources.Load<Sprite>($"{resourcePrefix}{i}");
            if (s != null)
            {
                frames.Add(s);
            }
        }

        for (int i = 1; i <= attackFrameCount; i++)
        {
            Sprite s = Resources.Load<Sprite>($"{attackResourcePrefix}{i}");
            if (s != null)
            {
                attackFrames.Add(s);
            }
        }

        if (frames.Count == 0)
        {
            Debug.LogWarning($"FinalBossAnimator: no frames at Resources/{resourcePrefix}1..{frameCount}");
            return;
        }

        // Render on a child so scaling the visual does not scale the root's
        // collider / rigidbody.
        Transform visual = transform.Find("BossVisual");
        if (visual == null)
        {
            visual = new GameObject("BossVisual").transform;
            visual.SetParent(transform, false);
        }

        // Remove any stray SpriteRenderer the setup added to the root.
        SpriteRenderer rootSr = GetComponent<SpriteRenderer>();
        if (rootSr != null)
        {
            Destroy(rootSr);
        }

        spriteRenderer = visual.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = visual.gameObject.AddComponent<SpriteRenderer>();
        }

        float refHeight = 0f;
        foreach (Sprite s in frames)
        {
            refHeight = Mathf.Max(refHeight, s.bounds.size.y);
        }

        float scale = refHeight > 0f ? targetWorldHeight / refHeight : 1f;
        visual.localScale = new Vector3(scale, scale, 1f);
        visual.localPosition = new Vector3(0f, visualYOffset, 0f);

        spriteRenderer.drawMode = SpriteDrawMode.Simple;
        spriteRenderer.sortingOrder = 10;
        spriteRenderer.sprite = frames[0];
        SpriteLit.Apply(spriteRenderer);
    }

    private void Update()
    {
        if (frames.Count == 0 || spriteRenderer == null || PauseMenu.IsPaused)
        {
            return;
        }

        if (boss != null)
        {
            spriteRenderer.flipX = boss.FacingDirection < 0;
        }

        // Melee attack OR distance shoot: both use the 3 "stand then strike"
        // frames. The attack art faces the opposite way to the walk art, so
        // invert flipX here to keep it facing the player.
        if (boss != null && (boss.IsAttacking || boss.IsShooting) && attackFrames.Count > 0)
        {
            spriteRenderer.flipX = boss.FacingDirection > 0;

            float progress = boss.IsShooting ? boss.ShootProgress01 : boss.AttackProgress01;
            int ai = Mathf.Clamp(
                Mathf.FloorToInt(progress * attackFrames.Count),
                0, attackFrames.Count - 1);
            spriteRenderer.sprite = attackFrames[ai];
            frameTimer = 0f;
            frameIndex = 0;
            return;
        }

        bool moving = boss == null || boss.IsMoving;
        if (!moving)
        {
            frameTimer = 0f;
            frameIndex = 0;
            spriteRenderer.sprite = frames[0];
            return;
        }

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % frames.Count;
            spriteRenderer.sprite = frames[frameIndex];
        }
    }
}
