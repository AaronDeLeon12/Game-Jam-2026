using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private float fireInterval = 1.5f;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private float projectileRange = 18f;
    [SerializeField] private float projectileDamage = 10f;

    private float nextFireTime;
    private EnemyHealth2D health;
    private Transform player;

    private void Awake()
    {
        health = GetComponent<EnemyHealth2D>();
        FindPlayer();
    }

    private void Update()
    {
        if (PauseMenu.IsPaused)
        {
            return;
        }

        if (health != null && !health.IsAlive)
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        if (player == null)
        {
            FindPlayer();
        }

        if (player != null
            && Vector2.Distance(transform.position, player.position) > projectileRange * DifficultyRules.EnemyRangeMultiplier)
        {
            return;
        }

        nextFireTime = Time.time + fireInterval * DifficultyRules.EnemyCooldownMultiplier;
        FireLeft();
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void FireLeft()
    {
        GameObject projectile = new GameObject("Beta Enemy Projectile");
        projectile.transform.position = transform.position + Vector3.left * 0.75f;
        projectile.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

        SpriteRenderer renderer = projectile.AddComponent<SpriteRenderer>();
        renderer.sprite = PlaceholderSprites.Square;
        renderer.color = new Color(0.35f, 1f, 0.25f);
        SpriteLit.Apply(renderer);
        renderer.sortingOrder = 20;

        BoxCollider2D collider = projectile.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Rigidbody2D body = projectile.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        EnemyProjectile enemyProjectile = projectile.AddComponent<EnemyProjectile>();
        enemyProjectile.Launch(Vector2.left, projectileSpeed, projectileRange, projectileDamage);
    }
}
