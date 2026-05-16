using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float gravityScale = 4.5f;

    private Rigidbody2D body;
    private Collider2D playerCollider;
    private float horizontalInput;
    private bool jumpRequested;
    private int facingDirection = 1;

    public int FacingDirection => facingDirection;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
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

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        Vector2 velocity = body.linearVelocity;
        velocity.x = horizontalInput * moveSpeed;

        if (jumpRequested)
        {
            velocity.y = jumpForce;
            jumpRequested = false;
        }
        else if (velocity.y < 0f)
        {
            velocity.y += Physics2D.gravity.y * (gravityScale * 0.4f) * Time.fixedDeltaTime;
        }

        body.linearVelocity = velocity;
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
