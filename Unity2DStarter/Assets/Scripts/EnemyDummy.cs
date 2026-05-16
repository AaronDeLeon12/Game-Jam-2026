using System.Collections;
using UnityEngine;

public class EnemyDummy : MonoBehaviour
{
    [SerializeField] private int maxHits = 3;
    [SerializeField] private float respawnDelay = 3f;

    private int hitsRemaining;
    private SpriteRenderer spriteRenderer;
    private Collider2D bodyCollider;

    private void Awake()
    {
        RefreshComponents();
        hitsRemaining = maxHits;
    }

    public void TakeHit()
    {
        RefreshComponents();

        if (hitsRemaining <= 0)
        {
            return;
        }

        hitsRemaining--;

        if (spriteRenderer != null)
        {
            float healthPercent = hitsRemaining / (float)maxHits;
            spriteRenderer.color = Color.Lerp(new Color(0.35f, 0.1f, 0.1f), new Color(0.2f, 0.9f, 0.25f), healthPercent);
        }

        if (hitsRemaining <= 0)
        {
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        SetVisible(false);
        yield return new WaitForSeconds(respawnDelay);
        hitsRemaining = maxHits;
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
