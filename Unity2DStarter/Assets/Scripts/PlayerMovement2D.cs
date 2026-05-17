using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpHeight = 2.5f;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float gravityScale = 5f;
    [SerializeField] private float fallGravityMultiplier = 1.7f;
    [SerializeField] private float glideGravityScale = 1.25f;
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashDisappearTime = 0.4f;
    [SerializeField] private float dashCooldown = 1.5f;

    [Header("Duck Settings")]
    [SerializeField] private float duckSpeedMultiplier = 0.5f;
    [SerializeField] private float duckHitboxHeightMultiplier = 0.5f;

    [Header("Duck Visual Settings")]
    [SerializeField] private float duckIdleVisualHeightMultiplier = 0.68f;
    [SerializeField] private float duckIdleVisualLowerAmount = 0.31f;

    [SerializeField] private float duckMoveVisualHeightMultiplier = 0.96f;
    [SerializeField] private float duckMoveVisualLowerAmount = 0.38f;

    [SerializeField] private float platformDropDuration = 0.35f;

    private Rigidbody2D body;
    private Collider2D playerCollider;
    private BoxCollider2D boxCollider;
    private SpriteRenderer playerRenderer;
    private PlayerStats playerStats;
    private PlayerActionCounter actionCounter;
    private PlayerSpriteAnimator spriteAnimator;

    private float horizontalInput;
    private bool jumpRequested;
    private bool dashRequested;
    private bool isDashing;
    private bool isDucking;
    private bool hasDoubleJumped;
    private bool wasGliding;
    private float nextDashTime;
    private int facingDirection = 1;

    private Vector2 standingColliderSize;
    private Vector2 standingColliderOffset;
    private Vector2 standingSpriteSize;
    private Vector3 standingRendererLocalPosition;
    private Vector3 standingRendererLocalScale;

    public int FacingDirection => facingDirection;
    public bool IsDucking => isDucking;
    public bool IsGroundedNow => playerCollider != null && IsGrounded();

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerRenderer = GetComponentInChildren<SpriteRenderer>();
        playerStats = GetComponent<PlayerStats>();
        actionCounter = GetComponent<PlayerActionCounter>();
        spriteAnimator = GetComponent<PlayerSpriteAnimator>();

        body.gravityScale = gravityScale;
        body.freezeRotation = true;

        if (boxCollider != null)
        {
            standingColliderSize = boxCollider.size;
            standingColliderOffset = boxCollider.offset;
        }

        if (playerRenderer != null)
        {
            if (!HasAnimatedPlayerArt())
            {
                playerRenderer.drawMode = SpriteDrawMode.Sliced;
            }

            standingSpriteSize = playerRenderer.size;
            standingRendererLocalPosition = playerRenderer.transform.localPosition;
            standingRendererLocalScale = playerRenderer.transform.localScale;
        }
    }

    private void Update()
    {
        if (PauseMenu.IsPaused || GameModal.IsOpen || (playerStats != null && playerStats.IsDead))
        {
            horizontalInput = 0f;
            jumpRequested = false;
            dashRequested = false;
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        bool isGrounded = IsGrounded();

        UpdateDuckingState(isGrounded);

        bool isBlocking = playerStats != null && playerStats.IsBlocking;

        if (Input.GetKeyDown(KeyCode.S) && isGrounded)
        {
            TryDropThroughPlatform();
        }

        if (isBlocking)
        {
            horizontalInput = 0f;
        }

        if (horizontalInput > 0.01f)
        {
            facingDirection = 1;
        }
        else if (horizontalInput < -0.01f)
        {
            facingDirection = -1;
        }

        if (isGrounded)
        {
            hasDoubleJumped = false;
        }

        if (JumpWasPressed() && isGrounded && !isDucking && !isBlocking && !HomeMode.IsActive)
        {
            jumpRequested = true;
            RecordAction("jump");
            GameAudio.PlaySfx("jumpsf", transform.position, 0.75f);
        }
        else if (JumpWasPressed() && !isGrounded && !hasDoubleJumped && !isBlocking && !HomeMode.IsActive)
        {
            jumpRequested = true;
            hasDoubleJumped = true;
            RecordAction("double_jump");
            GameAudio.PlaySfx("doubleJumpSF", transform.position, 0.75f);
        }

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            && Time.time >= nextDashTime
            && !isBlocking
            && !HomeMode.IsActive)
        {
            if (DifficultyRules.DashHasManaCost
                && playerStats != null
                && !playerStats.TryPayCost(5f, 0f))
            {
                return;
            }

            dashRequested = true;
            RecordAction("dash");
        }
    }

    private void FixedUpdate()
    {
        if (playerStats != null && playerStats.IsDead)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        if (isDashing)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        bool isGrounded = IsGrounded();
        bool isBlocking = playerStats != null && playerStats.IsBlocking;

        bool isGliding = hasDoubleJumped
            && !isGrounded
            && JumpIsHeld()
            && !isBlocking
            && body.linearVelocity.y <= 0f;

        if (isGliding && !wasGliding)
        {
            GameAudio.PlaySfx("glideSF", transform.position, 0.45f);
        }

        wasGliding = isGliding;
        body.gravityScale = isGliding ? glideGravityScale : gravityScale;

        float currentMoveSpeed = isDucking ? moveSpeed * duckSpeedMultiplier : moveSpeed;

        if (HomeMode.IsActive)
        {
            currentMoveSpeed *= HomeMode.MoveSpeedMultiplier;
        }

        Vector2 velocity = body.linearVelocity;
        velocity.x = isBlocking ? 0f : horizontalInput * currentMoveSpeed;

        if (jumpRequested)
        {
            velocity.y = CalculateJumpVelocity();
            jumpRequested = false;
        }
        else if (!isGliding && velocity.y < 0f)
        {
            velocity.y += Physics2D.gravity.y * gravityScale * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        body.linearVelocity = velocity;

        if (dashRequested)
        {
            dashRequested = false;
            StartCoroutine(DashForward());
        }
    }

    private float CalculateJumpVelocity()
    {
        return Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * gravityScale * jumpHeight);
    }

    private static bool JumpWasPressed()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W);
    }

    private static bool JumpIsHeld()
    {
        return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W);
    }

    private void RecordAction(string actionName)
    {
        if (actionCounter == null)
        {
            actionCounter = GetComponent<PlayerActionCounter>();
        }

        if (actionCounter != null)
        {
            actionCounter.Record(actionName);
        }
    }

    private void ApplyDuckHitbox()
    {
        if (boxCollider == null)
        {
            return;
        }

        RefreshPlayerRenderer();

        if (standingColliderSize == Vector2.zero)
        {
            standingColliderSize = boxCollider.size;
            standingColliderOffset = boxCollider.offset;
        }

        if (isDucking)
        {
            float duckHeight = standingColliderSize.y * duckHitboxHeightMultiplier;
            float heightDifference = standingColliderSize.y - duckHeight;

            boxCollider.size = new Vector2(standingColliderSize.x, duckHeight);
            boxCollider.offset = standingColliderOffset + Vector2.down * heightDifference * 0.5f;

            ApplyDuckVisual();
        }
        else
        {
            boxCollider.size = standingColliderSize;
            boxCollider.offset = standingColliderOffset;
            RestoreStandingVisual();
        }
    }

    private void ApplyDuckVisual()
    {
        if (playerRenderer == null)
        {
            return;
        }

        bool isCrouchMoving = Mathf.Abs(horizontalInput) > 0.01f;

        float currentDuckVisualHeight = isCrouchMoving
            ? duckMoveVisualHeightMultiplier
            : duckIdleVisualHeightMultiplier;

        float currentDuckVisualLower = isCrouchMoving
            ? duckMoveVisualLowerAmount
            : duckIdleVisualLowerAmount;

        if (HasAnimatedPlayerArt())
        {
            playerRenderer.transform.localScale = new Vector3(
                standingRendererLocalScale.x,
                standingRendererLocalScale.y * currentDuckVisualHeight,
                standingRendererLocalScale.z
            );

            playerRenderer.transform.localPosition =
                standingRendererLocalPosition + Vector3.down * currentDuckVisualLower;
        }
        else
        {
            playerRenderer.size = new Vector2(
                standingSpriteSize.x,
                standingSpriteSize.y * currentDuckVisualHeight
            );

            playerRenderer.transform.localPosition =
                standingRendererLocalPosition + Vector3.down * currentDuckVisualLower;
        }
    }

    private void RestoreStandingVisual()
    {
        if (playerRenderer == null)
        {
            return;
        }

        if (HasAnimatedPlayerArt())
        {
            playerRenderer.transform.localScale = standingRendererLocalScale;
        }
        else
        {
            playerRenderer.size = standingSpriteSize;
        }

        playerRenderer.transform.localPosition = standingRendererLocalPosition;
    }

    private void UpdateDuckingState(bool isGrounded)
    {
        bool wantsToDuck = Input.GetKey(KeyCode.S);
        bool canStartDuck = isGrounded && IsNearlyStill();

        if (wantsToDuck && (isDucking || canStartDuck))
        {
            isDucking = true;
        }
        else if (!wantsToDuck && CanStandUp())
        {
            isDucking = false;
        }

        if (!isGrounded)
        {
            isDucking = false;
        }

        ApplyDuckHitbox();
    }

    private void RefreshPlayerRenderer()
    {
        if (playerRenderer != null)
        {
            return;
        }

        playerRenderer = GetComponentInChildren<SpriteRenderer>();

        if (playerRenderer == null)
        {
            return;
        }

        if (!HasAnimatedPlayerArt())
        {
            playerRenderer.drawMode = SpriteDrawMode.Sliced;
        }

        standingSpriteSize = playerRenderer.size;
        standingRendererLocalPosition = playerRenderer.transform.localPosition;
        standingRendererLocalScale = playerRenderer.transform.localScale;
    }

    private bool HasAnimatedPlayerArt()
    {
        PlayerSpriteAnimator animator = GetComponent<PlayerSpriteAnimator>();
        return animator != null && animator.HasLoadedSprites;
    }

    private bool IsNearlyStill()
    {
        return Mathf.Abs(horizontalInput) < 0.01f && Mathf.Abs(body.linearVelocity.x) < 0.05f;
    }

    private bool CanStandUp()
    {
        if (boxCollider == null)
        {
            return true;
        }

        Bounds currentBounds = boxCollider.bounds;

        float standingTop =
            transform.position.y + standingColliderOffset.y + standingColliderSize.y * 0.5f;

        float crouchTop = currentBounds.max.y;
        float headroomHeight = standingTop - crouchTop;

        if (headroomHeight <= 0.05f)
        {
            return true;
        }

        Vector2 checkCenter = new Vector2(
            transform.position.x + standingColliderOffset.x,
            crouchTop + headroomHeight * 0.5f
        );

        Vector2 checkSize = new Vector2(
            standingColliderSize.x * 0.85f,
            headroomHeight * 0.9f
        );

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            checkCenter,
            checkSize,
            0f,
            groundLayers
        );

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit == playerCollider || hit.isTrigger)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private float GetClampedDashDistance()
    {
        Bounds bounds = playerCollider.bounds;
        Vector2 size = new Vector2(bounds.size.x * 0.9f, bounds.size.y * 0.6f);
        Vector2 direction = Vector2.right * facingDirection;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            bounds.center,
            size,
            0f,
            direction,
            dashDistance,
            groundLayers
        );

        float clamped = dashDistance;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider == playerCollider)
            {
                continue;
            }

            if (IsEnemyCollider(hit.collider))
            {
                continue;
            }

            if (hit.collider.isTrigger
                || hit.collider.GetComponent<PlatformEffector2D>() != null
                || hit.collider.GetComponent<GroundSurface2D>() != null)
            {
                continue;
            }

            clamped = Mathf.Min(clamped, hit.distance);
        }

        return clamped;
    }

    private static bool IsEnemyCollider(Collider2D target)
    {
        return target.GetComponent<Enemy>() != null
            || target.GetComponent<EnemyHealth2D>() != null
            || target.GetComponent<EnemyDummy>() != null
            || target.GetComponent<FlyingEnemyAI>() != null
            || target.GetComponent<SmallContactEnemyAI>() != null
            || target.GetComponent<KitingShooterEnemyAI>() != null
            || target.GetComponent<HeavyEnemyAI>() != null;
    }

    private IEnumerator DashForward()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        body.linearVelocity = Vector2.zero;
        body.simulated = false;

        float distance = GetClampedDashDistance();
        float phaseDuration = dashDisappearTime * 0.5f;

        if (spriteAnimator != null)
        {
            GameAudio.PlaySfx("teleportSF", transform.position, 0.85f);
            yield return spriteAnimator.PlayTeleportVanish(phaseDuration);
        }
        else
        {
            GameAudio.PlaySfx("teleportSF", transform.position, 0.85f);
            yield return new WaitForSeconds(phaseDuration);
        }

        transform.position += Vector3.right * facingDirection * distance;
        body.simulated = true;

        if (spriteAnimator != null)
        {
            GameAudio.PlaySfx("teleportSF", transform.position, 0.85f);
            yield return spriteAnimator.PlayTeleportAppear(phaseDuration);
        }
        else
        {
            GameAudio.PlaySfx("teleportSF", transform.position, 0.85f);
            yield return new WaitForSeconds(phaseDuration);
        }

        isDashing = false;
    }

    private void TryDropThroughPlatform()
    {
        Bounds bounds = playerCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y - 0.04f);
        Vector2 size = new Vector2(bounds.size.x * 0.85f, 0.08f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, size, 0f, groundLayers);

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit == playerCollider)
            {
                continue;
            }

            if (hit.GetComponent<PlatformEffector2D>() != null)
            {
                StartCoroutine(DropThrough(hit));
            }
        }
    }

    private IEnumerator DropThrough(Collider2D platform)
    {
        Physics2D.IgnoreCollision(playerCollider, platform, true);

        yield return new WaitForSeconds(platformDropDuration);

        float timeout = Time.time + 1f;

        while (platform != null
            && Time.time < timeout
            && playerCollider.bounds.min.y < platform.bounds.max.y)
        {
            yield return null;
        }

        if (platform != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platform, false);
        }
    }

    private bool IsGrounded()
    {
        Bounds bounds = playerCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y - 0.04f);
        Vector2 size = new Vector2(bounds.size.x * 0.85f, 0.08f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, size, 0f, groundLayers);

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit != playerCollider)
            {
                GroundSurface2D groundSurface = hit.GetComponent<GroundSurface2D>();

                if (groundSurface != null && bounds.min.y >= hit.bounds.max.y - 0.12f)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
