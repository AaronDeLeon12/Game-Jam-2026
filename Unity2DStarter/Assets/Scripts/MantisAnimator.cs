using System.Collections;
using UnityEngine;

/// <summary>
/// Drives the 2-frame walk cycle for the mantis enemy and handles the red
/// damage-flash overlay.
/// </summary>
public class MantisAnimator : MonoBehaviour
{
    [Header("Walk frames")]
    [SerializeField] private string walkResourcePrefix = "Enemies/mantis_walking/mantis_walk_";
    [SerializeField] private int walkFrameCount = 2;
    [SerializeField] private float framesPerSecond = 6f;

    [Header("Attack frames")]
    [SerializeField] private string attackResourcePrefix = "Enemies/mantis_walking/mantis_attack_";
    [SerializeField] private int attackFrameCount = 3;
    [Tooltip("Attack art is cropped tighter than the walk art, so scale it up while attacking to match.")]
    [SerializeField] private float attackScaleMultiplier = 1.2f;

    [Header("Size")]
    [SerializeField] private float targetWorldHeight = 1.8f;
    [SerializeField] private float visualYOffset = -0.3f;
    [SerializeField] private bool autoAlignFeet = true; // offset so feet sit on collider bottom

    [Header("Damage Flash")]
    [SerializeField] private float overlayMaxAlpha = 0.55f;
    [SerializeField] private int flashBlinkCount = 3;
    [SerializeField] private float flashBlinkHalfDuration = 0.07f;

    private MantisEnemy enemy;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer overlayRenderer;

    private Sprite[] walkFrames;
    private Sprite[] attackFrames;
    private Transform visualT;
    private float baseScale;

    private float frameTimer;
    private int frameIndex;

    private Coroutine activeFlash;

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        enemy = GetComponent<MantisEnemy>();

        // Load walk frames
        walkFrames = new Sprite[walkFrameCount];

        for (int i = 0; i < walkFrameCount; i++)
        {
            string path = $"{walkResourcePrefix}{i + 1}";

            walkFrames[i] = Resources.Load<Sprite>(path);

            Debug.Log($"Loading sprite: {path}");
            Debug.Log($"Loaded result: {walkFrames[i]}");

            if (walkFrames[i] == null)
            {
                Debug.LogError($"MantisAnimator: FAILED TO LOAD {path}");
            }
        }

        // Load attack frames
        attackFrames = new Sprite[attackFrameCount];
        for (int i = 0; i < attackFrameCount; i++)
        {
            attackFrames[i] = Resources.Load<Sprite>($"{attackResourcePrefix}{i + 1}");
            if (attackFrames[i] == null)
                Debug.LogWarning($"MantisAnimator: missing {attackResourcePrefix}{i + 1}");
        }

        SetupVisualChild();
    }

    // ─────────────────────────────────────────────────────────────
    private void SetupVisualChild()
    {
        // Remove root renderer if it exists
        SpriteRenderer rootSr = GetComponent<SpriteRenderer>();

        if (rootSr != null)
        {
            Destroy(rootSr);
        }

        // Create/find visual child
        Transform visual = transform.Find("MantisVisual");

        if (visual == null)
        {
            GameObject visualObj = new GameObject("MantisVisual");

            visual = visualObj.transform;
            visual.SetParent(transform, false);
        }

        // FORCE a fresh SpriteRenderer
        spriteRenderer = visual.gameObject.AddComponent<SpriteRenderer>();

        // Scale to target world height using first valid frame.
        float refH = 0f;
        foreach (Sprite s in walkFrames)
            if (s != null) refH = Mathf.Max(refH, s.bounds.size.y);
        float scale = refH > 0f ? targetWorldHeight / refH : 0.2f;
        visual.localScale = new Vector3(scale, scale, 1f);
        visualT = visual;
        baseScale = scale;

        // Auto-align feet: shift visual down so the sprite bottom sits on the
        // collider bottom edge (same as how the player / boss are positioned).
        float yOff = visualYOffset;
        if (autoAlignFeet)
        {
            Collider2D col = GetComponent<Collider2D>();
            float colHalfH = col != null ? col.bounds.extents.y : 0f;
            float spriteHalfH = targetWorldHeight * 0.5f;
            // move sprite center up by (spriteHalfH - colHalfH) so feet = collider bottom
            yOff += spriteHalfH - colHalfH;
        }
        visual.localPosition = new Vector3(0f, yOff, 0f);

        // Sorting
        spriteRenderer.sortingOrder = 10;

        // Force visible debug color
        spriteRenderer.color = Color.white;

        // Assign first sprite
        if (walkFrames[0] != null)
        {
            spriteRenderer.sprite = walkFrames[0];
        }

        // IMPORTANT:
        // Temporarily disabled because likely causing invisibility
        //
        // SpriteLit.Apply(spriteRenderer);

        // ───────────────── Overlay ─────────────────

        Transform overlayT = visual.Find("MantisOverlay");

        if (overlayT == null)
        {
            GameObject overlayObj = new GameObject("MantisOverlay");

            overlayT = overlayObj.transform;
            overlayT.SetParent(visual, false);
        }

        overlayRenderer = overlayT.gameObject.AddComponent<SpriteRenderer>();

        overlayRenderer.sortingOrder = 11;

        overlayRenderer.color = new Color(1f, 0f, 0f, 0f);

        if (walkFrames[0] != null)
        {
            overlayRenderer.sprite = walkFrames[0];
        }

        // Temporarily disabled
        //
        // SpriteLit.Apply(overlayRenderer);
    }

    // ─────────────────────────────────────────────────────────────
    private void Update()
    {
        if (spriteRenderer == null) return;

        if (PauseMenu.IsPaused) return;

        // Facing direction
        if (enemy != null)
        {
            spriteRenderer.flipX = enemy.FacingDirection < 0;
        }

        // Attack animation: map attack progress (0..1) across the 3 frames.
        if (enemy != null && enemy.IsAttacking && attackFrames != null && attackFrames.Length > 0)
        {
            int ai = Mathf.Clamp(
                Mathf.FloorToInt(enemy.AttackProgress01 * attackFrames.Length),
                0, attackFrames.Length - 1);
            if (attackFrames[ai] != null)
                spriteRenderer.sprite = attackFrames[ai];
            // Attack art is cropped tighter -> scale up to compensate.
            if (visualT != null)
            {
                float s = baseScale * attackScaleMultiplier;
                visualT.localScale = new Vector3(s, s, 1f);
            }
            frameTimer = 0f;
            frameIndex = 0;
            return;
        }

        // Not attacking: keep the visual at its normal walk scale.
        if (visualT != null && !Mathf.Approximately(visualT.localScale.x, baseScale))
        {
            visualT.localScale = new Vector3(baseScale, baseScale, 1f);
        }

        // Determine walking
        bool walking =
            enemy == null ||
            (enemy.IsMoving &&
             enemy.CurrentState == MantisEnemy.State.Chasing);

        // Idle frame
        if (!walking)
        {
            frameTimer = 0f;
            frameIndex = 0;

            if (walkFrames[0] != null)
            {
                spriteRenderer.sprite = walkFrames[0];
            }

            return;
        }

        // Animate
        frameTimer += Time.deltaTime;

        float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;

            frameIndex = (frameIndex + 1) % walkFrames.Length;
        }

        if (walkFrames[frameIndex] != null)
        {
            spriteRenderer.sprite = walkFrames[frameIndex];
        }
    }

    // ─────────────────────────────────────────────────────────────
    public void FlashDamage()
    {
        if (overlayRenderer == null) return;

        if (activeFlash != null)
        {
            StopCoroutine(activeFlash);
        }

        activeFlash = StartCoroutine(DamageFlashRoutine());
    }

    // ─────────────────────────────────────────────────────────────
    private IEnumerator DamageFlashRoutine()
    {
        if (overlayRenderer == null) yield break;

        Color on = new Color(1f, 0f, 0f, overlayMaxAlpha);
        Color off = new Color(1f, 0f, 0f, 0f);

        for (int i = 0; i < flashBlinkCount; i++)
        {
            if (spriteRenderer != null)
            {
                overlayRenderer.sprite = spriteRenderer.sprite;
                overlayRenderer.flipX = spriteRenderer.flipX;
            }

            overlayRenderer.color = on;

            yield return new WaitForSeconds(flashBlinkHalfDuration);

            overlayRenderer.color = off;

            yield return new WaitForSeconds(flashBlinkHalfDuration);
        }

        activeFlash = null;
    }
}