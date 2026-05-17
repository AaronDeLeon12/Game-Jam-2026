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
    private Transform player;
    private float nextFireTime;
    private float recoveryEndTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 5f;
        body.freezeRotation = true;
        FindPlayer();
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

        if (Time.time >= nextFireTime)
        {
            FireAtPlayer();
            nextFireTime = Time.time + fireInterval;
            recoveryEndTime = Time.time + recoveryDuration;
        }
    }

    private void KeepDistance()
    {
        float distance = player.position.x - transform.position.x;
        float absDistance = Mathf.Abs(distance);
        float direction = 0f;

        if (absDistance < preferredDistance - 0.75f)
        {
            direction = -Mathf.Sign(distance);
        }
        else if (absDistance > preferredDistance + 0.75f)
        {
            direction = Mathf.Sign(distance);
        }

        body.linearVelocity = new Vector2(direction * moveSpeed, body.linearVelocity.y);
    }

    private void FireAtPlayer()
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        GameObject projectile = new GameObject("Kiting Enemy Projectile");
        projectile.transform.position = transform.position + (Vector3)(direction * 0.75f);
        projectile.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

        SpriteRenderer renderer = projectile.AddComponent<SpriteRenderer>();
        renderer.sprite = PlaceholderSprites.Square;
        renderer.color = new Color(0.35f, 1f, 0.25f);
        renderer.sortingOrder = 20;

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
}
