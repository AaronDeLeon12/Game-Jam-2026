using UnityEngine;

/// <summary>
/// Final boss (the "oruga"/caterpillar). First behaviour pass: when it is on
/// the ground it crawls toward and follows the player. Attacks/phases come
/// later. The crawl visuals are driven by FinalBossAnimator, which reads
/// IsMoving / FacingDirection from this component.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FinalBoss : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 3000f;
    [SerializeField] private float moveSpeed = 3f; // 20% faster than the original 2.5
    [SerializeField] private float stopDistance = 0.8f;
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float groundCheckDistance = 0.7f;

    [Header("Melee Attack")]
    [SerializeField] private float attackRange = 3f;       // 3 tiles
    [SerializeField] private float attackDamage = 30f;
    [SerializeField] private float attackDuration = 0.9f;  // full animation
    [SerializeField] private float attackHitTime = 0.5f;   // when the hit lands
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Distance Attack")]
    [SerializeField] private float verticalAttackDistance = 3f;  // 3 tiles in Y
    [SerializeField] private float verticalHoldTime = 0.6f;        // sustained for 2s
    [SerializeField] private float horizontalAttackDistance = 4f;  // 4 tiles in X
    [SerializeField] private float horizontalAttackCooldown = 3f;  // anti-spam gate
    [SerializeField] private float shootDuration = 0.9f;
    [SerializeField] private float shootReleaseTime = 0.5f;      // when projectile spawns
    [SerializeField] private float distanceAttackCooldown = 2.5f;
    [SerializeField] private float projectileDamage = 30f;
    [SerializeField] private float projectileFlightTime = 0.6f;
    [SerializeField] private float projectileGravityScale = 0.8f;
    [SerializeField] private float projectileScale = 0.7f;
    [SerializeField] private float projectileSpawnYOffset = 1.4f;
    [SerializeField] private string projectileResource = "Enemies/oruga_projectile";
    [SerializeField] private int extraProjectileCount = 2;
    [SerializeField] private float extraGravityReduction = 2f;

    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private FinalBossAnimator animator;
    private Transform player;
    private PlayerStats playerStats;
    private int facingDirection = 1;
    private float health;

    private bool isAttacking;
    private float attackTimer;
    private bool attackHitApplied;
    private float nextAttackTime;

    private bool isShooting;
    private float shootTimer;
    private bool shotFired;
    private float verticalFarTimer;
    private float nextDistanceAttackTime;
    private float nextHorizontalAttackTime;

    public bool IsMoving { get; private set; }
    public bool IsAttacking => isAttacking;
    public float AttackProgress01 => attackDuration > 0f ? Mathf.Clamp01(attackTimer / attackDuration) : 0f;
    public bool IsShooting => isShooting;
    public float ShootProgress01 => shootDuration > 0f ? Mathf.Clamp01(shootTimer / shootDuration) : 0f;
    public int FacingDirection => facingDirection;
    public float Health => health;
    public float MaxHealth => maxHealth;

    public void TakeDamage(float amount)
    {
        health = Mathf.Max(0f, health - Mathf.Max(0f, amount));
        if (animator != null)
        {
            animator.FlashDamage();
        }
        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        animator = GetComponent<FinalBossAnimator>();
        body.freezeRotation = true;
        health = maxHealth;

        GameObject p = GameObject.Find("Player");
        if (p != null)
        {
            player = p.transform;
            playerStats = p.GetComponent<PlayerStats>();
        }
    }

    private void FixedUpdate()
    {
        if (PauseMenu.IsPaused || player == null)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            IsMoving = false;
            return;
        }

        // --- Distance attack in progress: stand, then release the projectile ---
        if (isShooting)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            IsMoving = false;
            shootTimer += Time.fixedDeltaTime;

            if (!shotFired && shootTimer >= shootReleaseTime)
            {
                shotFired = true;
                SpawnProjectile();
            }

            if (shootTimer >= shootDuration)
            {
                isShooting = false;
                verticalFarTimer = 0f;
                nextDistanceAttackTime = Time.time + distanceAttackCooldown;
            }

            return;
        }

        // --- Melee attack in progress: hold still, play it out, deal damage ---
        if (isAttacking)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            IsMoving = false;
            attackTimer += Time.fixedDeltaTime;

            if (!attackHitApplied && attackTimer >= attackHitTime)
            {
                attackHitApplied = true;
                if (playerStats != null
                    && Vector2.Distance(transform.position, player.position) <= attackRange * 1.4f)
                {
                    playerStats.TakeDamage(attackDamage);
                }
            }

            if (attackTimer >= attackDuration)
            {
                isAttacking = false;
                nextAttackTime = Time.time + attackCooldown;
            }

            return;
        }

        bool grounded = IsGrounded();
        float dx = player.position.x - transform.position.x;
        float distance = Vector2.Distance(transform.position, player.position);
        float verticalGap = Mathf.Abs(player.position.y - transform.position.y);

        // Track how long the player stays >= 3 tiles above/below in Y.
        if (verticalGap >= verticalAttackDistance)
        {
            verticalFarTimer += Time.fixedDeltaTime;
        }
        else
        {
            verticalFarTimer = 0f;
        }

        // Player too far vertically for 2s+ -> stand and shoot a parabola.
        if (grounded && verticalFarTimer >= verticalHoldTime && Time.time >= nextDistanceAttackTime)
        {
            facingDirection = dx >= 0f ? 1 : -1;
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            IsMoving = false;
            isShooting = true;
            shootTimer = 0f;
            shotFired = false;
            return;
        }

        // Player too far horizontally (> 4 tiles in X) -> stand and shoot,
        // gated by its own 3s cooldown so the boss does not spam it.
        if (grounded && Mathf.Abs(dx) > horizontalAttackDistance
            && Time.time >= nextHorizontalAttackTime)
        {
            facingDirection = dx >= 0f ? 1 : -1;
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            IsMoving = false;
            isShooting = true;
            shootTimer = 0f;
            shotFired = false;
            nextHorizontalAttackTime = Time.time + horizontalAttackCooldown;
            return;
        }

        // Player within 3 tiles and ready -> start the melee attack.
        if (grounded && distance <= attackRange && Time.time >= nextAttackTime)
        {
            facingDirection = dx >= 0f ? 1 : -1;
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            IsMoving = false;
            isAttacking = true;
            attackTimer = 0f;
            attackHitApplied = false;
            return;
        }

        if (grounded && Mathf.Abs(dx) > stopDistance)
        {
            int dir = dx >= 0f ? 1 : -1;
            facingDirection = dir;
            body.linearVelocity = new Vector2(dir * moveSpeed, body.linearVelocity.y);
            IsMoving = true;
        }
        else
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            IsMoving = false;
        }
    }

    private void SpawnProjectile()
    {
        // Main arced shot.
        LaunchOne(player.position, projectileFlightTime, projectileGravityScale);

        // Two extra shots that look the same but have much less gravity
        // (reduced by extraGravityReduction) so they fly faster and almost
        // straight. Small vertical spread so the volley fans out.
        float straightGravity = Mathf.Max(0.05f, projectileGravityScale - extraGravityReduction);
        float fastTime = projectileFlightTime * 0.6f;
        Vector3[] offsets = { new Vector3(0f, 0.9f, 0f), new Vector3(0f, -0.4f, 0f) };
        for (int i = 0; i < extraProjectileCount && i < offsets.Length; i++)
        {
            LaunchOne((Vector2)(player.position + offsets[i]), fastTime, straightGravity);
        }
    }

    private void LaunchOne(Vector2 target, float flightTime, float gravity)
    {
        Sprite sprite = Resources.Load<Sprite>(projectileResource);

        GameObject pr = new GameObject("Boss Projectile");
        pr.transform.position = transform.position
            + new Vector3(facingDirection * 0.5f, projectileSpawnYOffset, 0f);
        pr.transform.localScale = Vector3.one * projectileScale;

        SpriteRenderer sr = pr.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 20;
        sr.flipX = facingDirection < 0; // mirror when shooting left
        SpriteLit.Apply(sr);

        CircleCollider2D col = pr.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        Rigidbody2D rb = pr.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;

        BossProjectile bp = pr.AddComponent<BossProjectile>();
        bp.Launch(target, projectileDamage, flightTime, gravity);
    }

    private bool IsGrounded()
    {
        Bounds b = bodyCollider.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y - 0.04f);
        Vector2 size = new Vector2(b.size.x * 0.9f, Mathf.Max(0.1f, groundCheckDistance));
        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, size, 0f, groundLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit != bodyCollider && !hit.isTrigger)
            {
                return true;
            }
        }

        return false;
    }
}
