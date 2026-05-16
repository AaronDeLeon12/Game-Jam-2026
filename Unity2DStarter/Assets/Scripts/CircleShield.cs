using UnityEngine;

public class CircleShield : MonoBehaviour
{
    private float health = 50f;
    private float expireTime;

    public void Activate(float maxHealth, float duration)
    {
        health = maxHealth;
        expireTime = Time.time + duration;
    }

    private void Update()
    {
        if (Time.time >= expireTime)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= Mathf.Max(0f, damage);

        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
