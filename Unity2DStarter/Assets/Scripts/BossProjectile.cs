using UnityEngine;

/// <summary>
/// Parabolic projectile thrown by the final boss's distance attack. Launched
/// with an arc that lands on the player's position; damages PlayerStats on
/// contact, despawns on solid ground or after a lifetime.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossProjectile : MonoBehaviour
{
    private float damage = 30f;
    private float lifeUntil;

    public void Launch(Vector2 target, float dmg, float flightTime, float gravityScale)
    {
        damage = dmg;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScale;

        float t = Mathf.Max(0.2f, flightTime);
        float g = Physics2D.gravity.y * gravityScale; // negative
        Vector2 p0 = transform.position;

        float vx = (target.x - p0.x) / t;
        float vy = (target.y - p0.y - 0.5f * g * t * t) / t;
        rb.linearVelocity = new Vector2(vx, vy);

        lifeUntil = Time.time + t + 3f;
    }

    private void Update()
    {
        if (PauseMenu.IsPaused)
        {
            return;
        }

        if (Time.time >= lifeUntil)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(damage);
            Destroy(gameObject);
        }

        // Passes through platforms / ground / walls. It only ends on a player
        // hit or when its lifetime runs out (see Update).
    }
}
