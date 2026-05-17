using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class KitingShooterEnemyAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.35f;
    [SerializeField] private float preferredDistance = 6f;
    [SerializeField] private float fireInterval = 1.5f;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private float projectileRange = 18f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float recoveryDuration = 0.35f;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private float nextFireTime;
    private float recoveryEndTime;

    private void Awake()
    {
        SetupVisual();
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 5f;
        body.freezeRotation = true;
        FindPlayer();
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
            new[] { "unicorn1", "unicorn2", "unicorn3", "unicorn4", "unicorn5", "unicorn6" },
            8f,
            true,
            true,
            256f,
            18,
            14);
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
            return;
        }

        if (Time.time < recoveryEndTime)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            return;
        }

        KeepDistance();
        UpdateFacing();

        if (Time.time >= nextFireTime)
        {
            FireAtPlayer();
            nextFireTime = Time.time + fireInterval * DifficultyRules.EnemyCooldownMultiplier;
            recoveryEndTime = Time.time + recoveryDuration * DifficultyRules.EnemyCooldownMultiplier;
        }
    }

    private void KeepDistance()
    {
        float distance = player.position.x - transform.position.x;
        float absDistance = Mathf.Abs(distance);
        float direction = 0f;

        float adjustedPreferredDistance = preferredDistance * DifficultyRules.EnemyRangeMultiplier;

        if (absDistance < adjustedPreferredDistance - 0.75f)
        {
            direction = -Mathf.Sign(distance);
        }
        else if (absDistance > adjustedPreferredDistance + 0.75f)
        {
            direction = Mathf.Sign(distance);
        }

        body.linearVelocity = new Vector2(direction * moveSpeed * DifficultyRules.EnemyAggressionMultiplier, body.linearVelocity.y);
    }

    private void FireAtPlayer()
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        GameObject projectile = new GameObject("Kiting Enemy Projectile");
        projectile.transform.position = transform.position + (Vector3)(direction * 0.75f);
        projectile.transform.localScale = new Vector3(0.24f, 0.24f, 1f);

        SpriteRenderer renderer = projectile.AddComponent<SpriteRenderer>();
        Sprite[] frames = MagicAttackSprites.SquareFrames;
        renderer.sprite = MagicAttackSprites.FirstOrFallback(frames, PlaceholderSprites.Square);
        renderer.color = frames.Length > 0 ? Color.white : new Color(0.25f, 0.65f, 1f);
        renderer.flipX = direction.x > 0f;
        renderer.sortingOrder = 20;
        SpriteLit.Apply(renderer);

        if (frames.Length > 1)
        {
            projectile.AddComponent<AnimatedSprite2D>().Play(frames, 14f);
        }

        BoxCollider2D collider = projectile.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Rigidbody2D projectileBody = projectile.AddComponent<Rigidbody2D>();
        projectileBody.bodyType = RigidbodyType2D.Kinematic;
        projectileBody.gravityScale = 0f;

        EnemyProjectile enemyProjectile = projectile.AddComponent<EnemyProjectile>();
        enemyProjectile.Launch(direction, projectileSpeed, projectileRange, projectileDamage);
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void UpdateFacing()
    {
        if (spriteRenderer == null || player == null)
        {
            return;
        }

        float direction = Mathf.Abs(body.linearVelocity.x) > 0.05f
            ? body.linearVelocity.x
            : player.position.x - transform.position.x;
        if (Mathf.Abs(direction) > 0.05f)
        {
            spriteRenderer.flipX = direction < 0f;
        }
    }
}
