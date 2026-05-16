using UnityEngine;

public class CircleShield : MonoBehaviour
{
    private float maxHealth = 50f;
    private float health = 50f;
    private float expireTime;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Activate(float shieldHealth, float duration)
    {
        maxHealth = Mathf.Max(1f, shieldHealth);
        health = maxHealth;
        expireTime = Time.time + duration;
        UpdateVisual();
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
        UpdateVisual();

        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            return;
        }

        Color color = spriteRenderer.color;
        color.a = Mathf.Lerp(0.12f, 0.4f, Mathf.Clamp01(health / maxHealth));
        spriteRenderer.color = color;
    }
}
