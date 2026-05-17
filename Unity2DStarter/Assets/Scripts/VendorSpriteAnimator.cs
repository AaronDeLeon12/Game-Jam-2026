using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VendorSpriteAnimator : MonoBehaviour
{
    [SerializeField] private string sheetResourcePath = "Home/VendedorAnimacion";
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 2;
    [SerializeField] private float framesPerSecond = 6f;
    [SerializeField] private float targetWorldHeight = 2.2f;
    [SerializeField] private float idleHoldSeconds = 2.5f;

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private int frameIndex;
    private float frameTimer;
    private float holdTimer;
    private bool isHolding = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        frames = RuntimeSpriteCropper.LoadGridFrames(sheetResourcePath, columns, rows);
        if (frames.Length > 0)
        {
            ApplyFrame(0);
            SpriteLit.Apply(spriteRenderer);
        }
    }

    private void Update()
    {
        if (PauseMenu.IsPaused || frames == null || frames.Length == 0)
        {
            return;
        }

        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer < idleHoldSeconds)
            {
                return;
            }

            holdTimer = 0f;
            frameTimer = 0f;
            isHolding = false;
        }

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % frames.Length;
            ApplyFrame(frameIndex);

            if (frameIndex == 0)
            {
                isHolding = true;
                holdTimer = 0f;
                return;
            }
        }
    }

    private void ApplyFrame(int index)
    {
        Sprite sprite = frames[index];
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 6;

        float height = sprite.bounds.size.y;
        if (height > 0f)
        {
            float scale = targetWorldHeight / height;
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
