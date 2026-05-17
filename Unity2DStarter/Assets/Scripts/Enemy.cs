using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Ranges")]
    [SerializeField] private float sightRange = 6f;
    [SerializeField] private float attackRange = 1.2f;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float timeBetweenAttacks = 1.5f;

    [Header("Contact Damage")]
    [SerializeField] private int contactDamage = 5;
    [SerializeField] private float contactDamageCooldown = 1f;

    [Header("Patrol Probes")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float edgeCheckDistance = 0.6f;
    [SerializeField] private float wallCheckDistance = 0.5f;

    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private Transform player;
    private PlayerStats playerStats;

    private float health;
    private int facingDirection = 1;
    private bool alreadyAttacked;
    private bool aggroed;
    private float nextContactDamageTime;
    private float baseMoveSpeed;
    private float baseSightRange;
    private float baseAttackRange;
    private float baseTimeBetweenAttacks;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        body.freezeRotation = true;

        baseMoveSpeed = moveSpeed;
        baseSightRange = sightRange;
        baseAttackRange = attackRange;
        baseTimeBetweenAttacks = timeBetweenAttacks;
        health = maxHealth;

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerStats = playerObj.GetComponent<PlayerStats>();
        }
    }

    private void Update()
    {
        if (PauseMenu.IsPaused)
        {
            return;
        }

        if (player == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool playerInSightRange = distanceToPlayer <= baseSightRange * DifficultyRules.EnemyRangeMultiplier;
        bool playerInAttackRange = distanceToPlayer <= baseAttackRange * DifficultyRules.EnemyRangeMultiplier;

        if (playerInAttackRange)
            AttackPlayer();
        else if (playerInSightRange || aggroed)
            ChasePlayer();
        else
            Patrol();
    }

    private void Patrol()
    {
        if (IsWallAhead() || !IsGroundAhead())
            Flip();

        body.linearVelocity = new Vector2(facingDirection * GetMoveSpeed(), body.linearVelocity.y);
    }

    private void ChasePlayer()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        SetFacing((int)direction);
        body.linearVelocity = new Vector2(direction * GetMoveSpeed(), body.linearVelocity.y);
    }

    private void AttackPlayer()
    {
        body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
        SetFacing(player.position.x >= transform.position.x ? 1 : -1);

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;

            if (playerStats != null)
                EnemyDamage2D.TryDamagePlayer(player.gameObject, attackDamage);

            Invoke(nameof(ResetAttack), baseTimeBetweenAttacks * DifficultyRules.EnemyCooldownMultiplier);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        aggroed = true;

        HitFlash2D.Play(gameObject, Color.white, 0.06f);
        GameAudio.PlaySfx("hitSFX", transform.position, 0.65f);

        if (health <= 0f)
        {
            SessionStats.Record("enemies_killed");
            Destroy(gameObject);
        }
    }

    private float GetMoveSpeed()
    {
        return baseMoveSpeed * DifficultyRules.EnemyAggressionMultiplier;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < nextContactDamageTime) return;
        if (playerStats == null || collision.gameObject != player.gameObject) return;

        EnemyDamage2D.TryDamagePlayer(player.gameObject, contactDamage);
        nextContactDamageTime = Time.time + contactDamageCooldown;
    }

    private bool IsGroundAhead()
    {
        Vector2 origin = new Vector2(
            bodyCollider.bounds.center.x + facingDirection * bodyCollider.bounds.extents.x,
            bodyCollider.bounds.min.y);
        return Physics2D.Raycast(origin, Vector2.down, edgeCheckDistance, groundLayer);
    }

    private bool IsWallAhead()
    {
        Vector2 origin = bodyCollider.bounds.center;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * facingDirection,
            bodyCollider.bounds.extents.x + wallCheckDistance, groundLayer);
        return hit.collider != null && hit.collider != bodyCollider;
    }

    private void Flip()
    {
        SetFacing(-facingDirection);
    }

    private void SetFacing(int direction)
    {
        if (direction == 0 || direction == facingDirection) return;

        facingDirection = direction;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facingDirection;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
