using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector2 direction = Vector2.left;
    private float speed = 7f;
    private float range = 18f;
    private float damage = 10f;
    private Vector3 startPosition;

    public void Launch(Vector2 launchDirection, float launchSpeed, float launchRange, float launchDamage)
    {
        direction = launchDirection.sqrMagnitude > 0f ? launchDirection.normalized : Vector2.left;
        speed = launchSpeed;
        range = launchRange;
        damage = launchDamage;
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CircleShield shield = other.GetComponent<CircleShield>();
        if (shield != null)
        {
            shield.TakeDamage(DifficultyRules.AdjustEnemyDamage(damage));
            Destroy(gameObject);
            return;
        }

        if (EnemyDamage2D.TryDamagePlayer(other, damage))
        {
            Destroy(gameObject);
        }
    }
}
