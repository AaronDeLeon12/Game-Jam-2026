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
    [SerializeField] private float dashDisappearTime = 0.12f;
    [SerializeField] private float dashCooldown = 1.5f;
    [SerializeField] private float dashManaCost = 10f;
    [SerializeField] private float doubleJumpManaCost = 5f;
    [SerializeField] private float movementManaRegenDelay = 0.3f;
    [SerializeField] private float duckSpeedMultiplier = 0.5f;
    [SerializeField] private float duckHitboxHeightMultiplier = 0.5f;
    [SerializeField] private float platformDropDuration = 0.35f;

    private Rigidbody2D body;
    private Collider2D playerCollider;
    private BoxCollider2D boxCollider;
    private SpriteRenderer playerRenderer;
    private PlayerStats playerStats;
    private float horizontalInput;
    private bool jumpRequested;
    private bool dashRequested;
    private bool isDashing;
    private bool isDucking;
    private bool hasDoubleJumped;
    private float nextDashTime;
    private int facingDirection = 1;
    private Vector2 standingColliderSize;
    private Vector2 standingColliderOffset;
    private Vector2 standingSpriteSize;
    private Vector3 standingRendererLocalPosition;

    public int FacingDirection => facingDirection;
    public bool IsDucking => isDucking;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerRenderer = GetComponentInChildren<SpriteRenderer>();
        playerStats = GetComponent<PlayerStats>();
        body.gravityScale = gravityScale;
        body.freezeRotation = true;

        if (boxCollider != null)
        {
            standingColliderSize = boxCollider.size;
            standingColliderOffset = boxCollider.offset;
        }

        if (playerRenderer != null)
        {
            playerRenderer.drawMode = SpriteDrawMode.Sliced;
            standingSpriteSize = playerRenderer.size;
            standingRendererLocalPosition = playerRenderer.transform.localPosition;
        }
    }

    private void Update()
    {
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

        if (JumpWasPressed() && isGrounded && !isDucking && !isBlocking)
        {
            jumpRequested = true;
        }
        else if (JumpWasPressed() && !isGrounded && !hasDoubleJumped && !isBlocking && TryPayMovementCost(doubleJumpManaCost))
        {
            jumpRequested = true;
            hasDoubleJumped = true;
        }

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            && Time.time >= nextDashTime
            && !isBlocking
            && TryPayMovementCost(dashManaCost))
        {
            dashRequested = true;
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        bool isGrounded = IsGrounded();
        bool isBlocking = playerStats != null && playerStats.IsBlocking;
        bool isGliding = hasDoubleJumped && !isGrounded && JumpIsHeld() && !isBlocking && body.linearVelocity.y <= 0f;
        body.gravityScale = isGliding ? glideGravityScale : gravityScale;
        float currentMoveSpeed = isDucking ? moveSpeed * duckSpeedMultiplier : moveSpeed;

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

    private bool TryPayMovementCost(float manaCost)
    {
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }

        return playerStats == null || playerStats.TryPayCost(manaCost, movementManaRegenDelay);
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

            if (playerRenderer != null)
            {
                playerRenderer.size = new Vector2(standingSpriteSize.x, standingSpriteSize.y * duckHitboxHeightMultiplier);
                playerRenderer.transform.localPosition = standingRendererLocalPosition + Vector3.down * heightDifference * 0.5f;
            }
        }
        else
        {
            boxCollider.size = standingColliderSize;
            boxCollider.offset = standingColliderOffset;

            if (playerRenderer != null)
            {
                playerRenderer.size = standingSpriteSize;
                playerRenderer.transform.localPosition = standingRendererLocalPosition;
            }
        }
    }

    private void UpdateDuckingState(bool isGrounded)
    {
        bool wantsToDuck = Input.GetKey(KeyCode.S);
        bool canStartDuck = isGrounded && IsNearlyStill();

        if (wantsToDuck && (isDucking || canStartDuck))
        {
            isDucking = true;
        }
        else if (!wantsToDuck && (!isDucking || CanStandUp()))
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

        playerRenderer.drawMode = SpriteDrawMode.Sliced;
        standingSpriteSize = playerRenderer.size;
        standingRendererLocalPosition = playerRenderer.transform.localPosition;
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

        Vector2 center = (Vector2)transform.position + standingColliderOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, standingColliderSize * 0.95f, 0f, groundLayers);

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit != playerCollider && !hit.isTrigger)
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator DashForward()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        body.linearVelocity = Vector2.zero;
        body.simulated = false;

        yield return new WaitForSeconds(dashDisappearTime);

        transform.position += Vector3.right * facingDirection * dashDistance;
        body.simulated = true;

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.enabled = true;
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
