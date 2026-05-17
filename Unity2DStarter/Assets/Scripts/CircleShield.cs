using UnityEngine;

public class CircleShield : MonoBehaviour
{
    private const float IntroDuration = 0.2f;

    private float maxHealth = 20f;
    private float health = 20f;
    private float duration = 2f;
    private float startTime;
    private float expireTime;
    private float endDuration = 0.4f;
    private SpriteRenderer spriteRenderer;
    private Sprite[] animationFrames;
    private int lastFrameIndex = -1;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Activate(float shieldHealth, float shieldDuration)
    {
        maxHealth = Mathf.Max(1f, shieldHealth);
        health = maxHealth;
        duration = Mathf.Max(0.5f, shieldDuration);
        startTime = Time.time;
        expireTime = startTime + duration;
        animationFrames = MagicAttackSprites.ShieldFrames;
        lastFrameIndex = -1;
        ApplyFrame(0);
        UpdateVisual();
    }

    private void Update()
    {
        UpdateAnimation();

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
        color.a = Mathf.Lerp(0.65f, 0.9f, Mathf.Clamp01(health / maxHealth));
        spriteRenderer.color = color;
    }

    private void UpdateAnimation()
    {
        if (animationFrames == null || animationFrames.Length == 0)
        {
            return;
        }

        float elapsed = Time.time - startTime;
        float remaining = expireTime - Time.time;
        int frameIndex;

        if (elapsed < IntroDuration && animationFrames.Length >= 2)
        {
            frameIndex = elapsed < IntroDuration * 0.5f ? 0 : 1;
        }
        else if (remaining <= endDuration && animationFrames.Length >= 2)
        {
            float endProgress = Mathf.Clamp01(1f - (remaining / endDuration));
            frameIndex = endProgress < 0.5f ? 1 : 0;
        }
        else if (animationFrames.Length > 2)
        {
            int loopFrameCount = animationFrames.Length - 2;
            float loopTime = Mathf.Max(0f, elapsed - IntroDuration);
            frameIndex = 2 + (Mathf.FloorToInt(loopTime * 8f) % loopFrameCount);
        }
        else
        {
            frameIndex = 0;
        }

        ApplyFrame(frameIndex);
    }

    private void ApplyFrame(int frameIndex)
    {
        if (spriteRenderer == null || animationFrames == null || animationFrames.Length == 0)
        {
            return;
        }

        frameIndex = Mathf.Clamp(frameIndex, 0, animationFrames.Length - 1);
        if (lastFrameIndex == frameIndex)
        {
            return;
        }

        lastFrameIndex = frameIndex;
        spriteRenderer.sprite = animationFrames[frameIndex];
    }
}
