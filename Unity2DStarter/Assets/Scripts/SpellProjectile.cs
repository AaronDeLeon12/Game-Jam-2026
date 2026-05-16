using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    private int direction = 1;
    private float speed = 12f;
    private float range = 3f;
    private float damage = 1f;
    private Vector3 startPosition;

    public void Launch(int launchDirection, float launchSpeed, float launchRange, float launchDamage)
    {
        direction = launchDirection >= 0 ? 1 : -1;
        speed = launchSpeed;
        range = launchRange;
        damage = launchDamage;
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;

        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyDummy dummy = other.GetComponent<EnemyDummy>();
        if (dummy == null)
        {
            return;
        }

        dummy.TakeDamage(damage);
        Destroy(gameObject);
    }
}
