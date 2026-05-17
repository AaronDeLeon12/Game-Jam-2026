using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FlyingEnemyAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 17.5f;
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float homeLeashRadius = 8f;
    [SerializeField] private float dashDamage = 10f;
    [SerializeField] private float dashCooldown = 2.2f;
    [SerializeField] private float windupDuration = 0.45f;
    [SerializeField] private float dashDuration = 0.35f;
    [SerializeField] private float recoveryDuration = 0.9f;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer chargeGlowRenderer;
    private Transform player;
    private Vector3 homePosition;
    private Vector2 dashDirection;
    private float nextAttackTime;
    private float stateEndTime;
    private bool dealtDamageThisDash;
    private State state;

    private enum State
    {
        Patrol,
        Windup,
        Dash,
        Recover
    }

    private void Awake()
    {
        SetupVisual();
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        Collider2D bodyCollider = GetComponent<Collider2D>();
        bodyCollider.isTrigger = true;

        homePosition = transform.position;
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void SetupVisual()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sortingOrder = 12;
        spriteRenderer.color = Color.white;
        ResourceSheetAnimator2D animator = GetComponent<ResourceSheetAnimator2D>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<ResourceSheetAnimator2D>();
        }

        animator.ConfigureFrameFiles(
            "Enemies",
            new[] { "fairy1", "fairy2", "fairy3", "fairy4", "fairy5", "fairy6" },
            8f,
            true,
            false,
            256f,
            10,
            8);

        Transform glow = transform.Find("Charge Glow");
        if (glow == null)
        {
            glow = new GameObject("Charge Glow").transform;
            glow.SetParent(transform, false);
        }

        chargeGlowRenderer = glow.GetComponent<SpriteRenderer>();
        if (chargeGlowRenderer == null)
        {
            chargeGlowRenderer = glow.gameObject.AddComponent<SpriteRenderer>();
        }

        chargeGlowRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        chargeGlowRenderer.color = new Color(1f, 0.9f, 0.1f, 0.28f);
        chargeGlowRenderer.enabled = false;
        glow.localScale = new Vector3(1.08f, 1.08f, 1f);
    }

    private void Update()
    {
        if (PauseMenu.IsPaused)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        if (player == null)
        {
            FindPlayer();
        }

        switch (state)
        {
            case State.Windup:
                UpdateWindup();
                break;
            case State.Dash:
                UpdateDash();
                break;
            case State.Recover:
                UpdateRecover();
                break;
            default:
                UpdateMoveState();
                break;
        }
    }

    private void UpdateMoveState()
    {
        if (player != null && Vector2.Distance(player.position, homePosition) <= homeLeashRadius * DifficultyRules.EnemyRangeMultiplier)
        {
            body.linearVelocity = Vector2.zero;

            if (Time.time >= nextAttackTime)
            {
                state = State.Windup;
                stateEndTime = Time.time + windupDuration * DifficultyRules.EnemyCooldownMultiplier;
                dashDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
                body.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            state = State.Patrol;
            float patrolAngle = Time.time * 1.2f;
            Vector2 target = (Vector2)homePosition + new Vector2(Mathf.Cos(patrolAngle), Mathf.Sin(patrolAngle * 0.7f)) * patrolRadius;
            MoveToward(target, moveSpeed * 0.45f);
        }
    }

    private void UpdateWindup()
    {
        body.linearVelocity = Vector2.zero;
        if (Time.time < stateEndTime)
        {
            return;
        }

        state = State.Dash;
        dealtDamageThisDash = false;
        stateEndTime = Time.time + dashDuration;
        body.linearVelocity = dashDirection * moveSpeed * DifficultyRules.EnemyAggressionMultiplier;
    }

    private void UpdateDash()
    {
        if (Time.time >= stateEndTime)
        {
            EnterRecover();
        }
    }

    private void UpdateRecover()
    {
        body.linearVelocity = Vector2.zero;
        if (Time.time >= stateEndTime)
        {
            state = State.Patrol;
        }
    }

    private void LateUpdate()
    {
        UpdateFacing();
        UpdateChargeGlow();
    }

    private void UpdateFacing()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        float direction = body != null && Mathf.Abs(body.linearVelocity.x) > 0.05f
            ? body.linearVelocity.x
            : dashDirection.x;
        if (Mathf.Abs(direction) > 0.05f)
        {
            spriteRenderer.flipX = direction < 0f;
        }
    }

    private void UpdateChargeGlow()
    {
        if (chargeGlowRenderer == null || spriteRenderer == null)
        {
            return;
        }

        bool showGlow = state == State.Windup;
        chargeGlowRenderer.enabled = showGlow;
        if (!showGlow)
        {
            return;
        }

        chargeGlowRenderer.sprite = spriteRenderer.sprite;
        chargeGlowRenderer.flipX = spriteRenderer.flipX;
        float pulse = Mathf.PingPong(Time.time * 5f, 1f);
        chargeGlowRenderer.color = new Color(1f, 0.88f, 0.05f, Mathf.Lerp(0.18f, 0.34f, pulse));
    }

    private void EnterRecover()
    {
        body.linearVelocity = Vector2.zero;
        state = State.Recover;
        stateEndTime = Time.time + recoveryDuration;
        nextAttackTime = Time.time + dashCooldown * DifficultyRules.EnemyCooldownMultiplier;
    }

    private void MoveToward(Vector2 target, float speed)
    {
        Vector2 direction = target - (Vector2)transform.position;
        body.linearVelocity = direction.sqrMagnitude > 0.05f ? direction.normalized * speed : Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state != State.Dash || dealtDamageThisDash)
        {
            return;
        }

        if (EnemyDamage2D.TryDamagePlayer(other, dashDamage))
        {
            dealtDamageThisDash = true;
            EnterRecover();
        }
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }
}
