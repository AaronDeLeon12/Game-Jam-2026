using UnityEngine;

public class AnimatedSprite2D : MonoBehaviour
{
    [SerializeField] private float framesPerSecond = 12f;
    [SerializeField] private bool loop = true;

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private float timer;
    private int frameIndex;

    public void Play(Sprite[] animationFrames, float fps = 12f, bool shouldLoop = true)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        frames = animationFrames;
        framesPerSecond = fps;
        loop = shouldLoop;
        timer = 0f;
        frameIndex = 0;

        if (spriteRenderer != null && frames != null && frames.Length > 0)
        {
            spriteRenderer.sprite = frames[0];
        }
    }

    private void Update()
    {
        if (spriteRenderer == null || frames == null || frames.Length <= 1)
        {
            return;
        }

        timer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);
        while (timer >= frameDuration)
        {
            timer -= frameDuration;
            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                frameIndex = loop ? 0 : frames.Length - 1;
            }

            spriteRenderer.sprite = frames[frameIndex];
        }
    }
}
