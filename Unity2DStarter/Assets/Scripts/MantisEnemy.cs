using UnityEngine;

/// <summary>
/// AI controller for the mantis enemy. Wired to the "mantis_enemy" scene
/// object via AddComponent or directly in the Inspector.
///
/// States
/// ------
///  Chasing       – walks on the ground toward the player.
///  MeleeAttacking – placeholder (logic added later).
///
/// MantisAnimator reads IsMoving / FacingDirection / State to drive the
/// 2-frame walk cycle and the (future) attack animation.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(MantisAnimator))]
public class MantisEnemy : MonoBehaviour, IDamageable
{
    // ── Tuning ────────────────────────────────────────────────────────────
    [Header("Stats")]
    [SerializeField] private float maxHealth    = 80f;
    [SerializeField] private float moveSpeed    = 2.8f;

    [Header("Chase")]
    [SerializeField] private float stopDistance = 0.9f;   // tiles from player before stopping
    [SerializeField] private float chaseRange   = 12f;    // tiles: starts chasing when closer

    [Header("Melee Attack")]
    // Same range as the boss (3 tiles) but 30% faster (durations x0.7).
    [SerializeField] private float attackRange    = 2f;     // 2 tiles
    [SerializeField] private float attackDamage   = 15f;    // half the boss's 30
    [SerializeField] private float attackDuration = 0.63f;  // boss 0.9 * 0.7
    [SerializeField] private float attackHitTime  = 0.35f;  // boss 0.5 * 0.7
    [SerializeField] private float attackCooldown = 0.84f;  // boss 1.2 * 0.7

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer        = ~0;
    [SerializeField] private float groundCheckDistance    = 0.5f;

    // ── State machine ─────────────────────────────────────────────────────
    public enum State { Chasing, MeleeAttacking }

    public State CurrentState  { get; private set; } = State.Chasing;
    public bool  IsMoving      { get; private set; }
    public int   FacingDirection { get; private set; } = 1;
    public bool  IsAttacking   => CurrentState == State.MeleeAttacking;
    public float AttackProgress01 =>
        attackDuration > 0f ? Mathf.Clamp01(attackTimer / attackDuration) : 0f;
    public float Health        => health;
    public float MaxHealth     => maxHealth;

    // ── Private ───────────────────────────────────────────────────────────
    private Rigidbody2D   body;
    private Collider2D    bodyCollider;
    private MantisAnimator animator;
    private Transform     player;
    private PlayerStats   playerStats;
    private float         health;

    private float attackTimer;
    private bool  attackHitApplied;
    private float nextAttackTime;

    // ── IDamageable ───────────────────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        health = Mathf.Max(0f, health - Mathf.Max(0f, amount));
        if (animator != null) animator.FlashDamage();
        if (health <= 0f) Destroy(gameObject);
    }

    // ── Unity lifecycle ───────────────────────────────────────────────────
    private void Awake()
    {
        body          = GetComponent<Rigidbody2D>();
        bodyCollider  = GetComponent<Collider2D>();
        animator      = GetComponent<MantisAnimator>();
        body.freezeRotation = true;
        health        = maxHealth;

        GameObject p = GameObject.Find("Player");
        if (p != null)
        {
            player      = p.transform;
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

        switch (CurrentState)
        {
            case State.Chasing:        TickChasing();        break;
            case State.MeleeAttacking: TickMeleeAttacking(); break;
        }
    }

    // ── State ticks ───────────────────────────────────────────────────────
    private void TickChasing()
    {
        float dx       = player.position.x - transform.position.x;
        float distance = Vector2.Distance(transform.position, player.position);
        bool  grounded = IsGrounded();

        // Only chase if the player is within range.
        if (distance > chaseRange)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            IsMoving = false;
            return;
        }

        // Player within 3 tiles and cooldown ready -> start the melee attack.
        if (grounded && distance <= attackRange && Time.time >= nextAttackTime)
        {
            FacingDirection = dx >= 0f ? 1 : -1;
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            IsMoving = false;
            attackTimer = 0f;
            attackHitApplied = false;
            EnterState(State.MeleeAttacking);
            return;
        }

        // Walk toward the player.
        if (grounded && Mathf.Abs(dx) > stopDistance)
        {
            int dir        = dx >= 0f ? 1 : -1;
            FacingDirection = dir;
            body.linearVelocity = new Vector2(dir * moveSpeed, body.linearVelocity.y);
            IsMoving       = true;
        }
        else
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            IsMoving = false;
        }
    }

    private void TickMeleeAttacking()
    {
        // Same shape as the boss melee: hold still, play it out, deal the hit
        // once at attackHitTime, then return to chasing on a cooldown.
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
            nextAttackTime = Time.time + attackCooldown;
            EnterState(State.Chasing);
        }
    }

    // Transition helper (call from TickChasing once attack logic is ready).
    private void EnterState(State next)
    {
        CurrentState = next;
    }

    // ── Ground check ──────────────────────────────────────────────────────
    private bool IsGrounded()
    {
        Bounds  b      = bodyCollider.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y - 0.04f);
        Vector2 size   = new Vector2(b.size.x * 0.85f, Mathf.Max(0.1f, groundCheckDistance));

        foreach (Collider2D hit in Physics2D.OverlapBoxAll(origin, size, 0f, groundLayer))
        {
            if (hit != null && hit != bodyCollider && !hit.isTrigger)
                return true;
        }
        return false;
    }
}
