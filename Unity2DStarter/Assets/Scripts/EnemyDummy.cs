using System.Collections;
using UnityEngine;

public class EnemyDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float respawnDelay = 3f;

    private float health;
    private SpriteRenderer spriteRenderer;
    private Collider2D bodyCollider;

    public bool IsAlive => health > 0f;

    private void Awake()
    {
        RefreshComponents();
        health = maxHealth;
    }

    public void TakeHit()
    {
        TakeDamage(1f);
    }

    public void TakeDamage(float damage)
    {
        RefreshComponents();

        if (health <= 0f)
        {
            return;
        }

        health = Mathf.Max(0f, health - damage);

        if (spriteRenderer != null)
        {
            float healthPercent = health / maxHealth;
            spriteRenderer.color = Color.Lerp(new Color(0.35f, 0.1f, 0.1f), new Color(0.2f, 0.9f, 0.25f), healthPercent);
        }

        if (health <= 0f)
        {
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        SetVisible(false);
        yield return new WaitForSeconds(respawnDelay);
        health = maxHealth;
        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        RefreshComponents();

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
            spriteRenderer.color = new Color(0.2f, 0.9f, 0.25f);
        }

        if (bodyCollider != null)
        {
            bodyCollider.enabled = visible;
        }
    }

    private void RefreshComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (bodyCollider == null)
        {
            bodyCollider = GetComponent<Collider2D>();
        }
    }
}
