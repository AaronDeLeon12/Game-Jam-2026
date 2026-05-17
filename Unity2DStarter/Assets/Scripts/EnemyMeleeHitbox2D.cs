using UnityEngine;

public class EnemyMeleeHitbox2D : MonoBehaviour
{
    private float damage = 10f;
    private float expireTime;
    private bool dealtDamage;

    public void Configure(float hitDamage, float lifetime)
    {
        damage = hitDamage;
        expireTime = Time.time + lifetime;
    }

    private void Update()
    {
        if (Time.time >= expireTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (dealtDamage)
        {
            return;
        }

        if (EnemyDamage2D.TryDamagePlayer(other, damage))
        {
            dealtDamage = true;
        }
    }
}
