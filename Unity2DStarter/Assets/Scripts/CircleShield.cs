using UnityEngine;

public class CircleShield : MonoBehaviour
{
    private float maxHealth = 20f;
    private float health = 20f;
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
        HitFlash2D.Play(gameObject, Color.white, 0.06f);
        GameAudio.PlaySfx("ShieldBlockSFX", transform.position, 0.85f);
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
        color.a = Mathf.Lerp(0.12f, 0.2f, Mathf.Clamp01(health / maxHealth));
        spriteRenderer.color = color;
    }
}
