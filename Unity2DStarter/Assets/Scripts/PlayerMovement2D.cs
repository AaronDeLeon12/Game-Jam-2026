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

    private Rigidbody2D body;
    private Collider2D playerCollider;
    private PlayerStats playerStats;
    private float horizontalInput;
    private bool jumpRequested;
    private bool dashRequested;
    private bool isDashing;
    private bool hasDoubleJumped;
    private float nextDashTime;
    private int facingDirection = 1;

    public int FacingDirection => facingDirection;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        playerStats = GetComponent<PlayerStats>();
        body.gravityScale = gravityScale;
        body.freezeRotation = true;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0.01f)
        {
            facingDirection = 1;
        }
        else if (horizontalInput < -0.01f)
        {
            facingDirection = -1;
        }

        bool isGrounded = IsGrounded();

        if (isGrounded)
        {
            hasDoubleJumped = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpRequested = true;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !hasDoubleJumped && TryPayMovementCost(doubleJumpManaCost))
        {
            jumpRequested = true;
            hasDoubleJumped = true;
        }

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            && Time.time >= nextDashTime
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
        bool isGliding = hasDoubleJumped && !isGrounded && Input.GetKey(KeyCode.Space) && body.linearVelocity.y <= 0f;
        body.gravityScale = isGliding ? glideGravityScale : gravityScale;

        Vector2 velocity = body.linearVelocity;
        velocity.x = horizontalInput * moveSpeed;

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

    private bool TryPayMovementCost(float manaCost)
    {
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }

        return playerStats == null || playerStats.TryPayCost(manaCost, movementManaRegenDelay);
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
                return true;
            }
        }

        return false;
    }
}
