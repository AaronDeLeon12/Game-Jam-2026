using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ResourceSheetAnimator2D : MonoBehaviour
{
    [SerializeField] private string resourcePath;
    [SerializeField] private string[] frameNames;
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 3;
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool pingPong;
    [SerializeField] private int ignoreLastFrames;
    [SerializeField] private float pixelsPerUnit = 256f;
    [SerializeField] private int cropPadding = 6;
    [SerializeField] private Vector2 spritePivot = new Vector2(0.5f, 0.5f);

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private float timer;
    private int frameIndex;
    private int frameDirection = 1;

    public void Configure(string newResourcePath, int newColumns, int newRows, float fps, bool shouldLoop, int ignoredLastFrames = 0, float ppu = 256f, int padding = 6, Vector2? normalizedPivot = null)
    {
        Configure(newResourcePath, newColumns, newRows, fps, shouldLoop, false, ignoredLastFrames, ppu, padding, normalizedPivot);
    }

    public void Configure(string newResourcePath, int newColumns, int newRows, float fps, bool shouldLoop, bool shouldPingPong, int ignoredLastFrames = 0, float ppu = 256f, int padding = 6, Vector2? normalizedPivot = null)
    {
        resourcePath = newResourcePath;
        frameNames = null;
        columns = newColumns;
        rows = newRows;
        framesPerSecond = fps;
        loop = shouldLoop;
        pingPong = shouldPingPong;
        ignoreLastFrames = Mathf.Max(0, ignoredLastFrames);
        pixelsPerUnit = ppu;
        cropPadding = padding;
        spritePivot = normalizedPivot ?? new Vector2(0.5f, 0.5f);
        LoadFrames();
    }

    public void ConfigureFrameFiles(string folderPath, string[] newFrameNames, float fps, bool shouldLoop, bool shouldPingPong, float ppu = 256f, int padding = 6, int bottomPadding = 8)
    {
        resourcePath = folderPath;
        frameNames = newFrameNames;
        framesPerSecond = fps;
        loop = shouldLoop;
        pingPong = shouldPingPong;
        pixelsPerUnit = ppu;
        cropPadding = padding;
        LoadFrameFiles(bottomPadding);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        LoadFrames();
    }

    private void LoadFrames()
    {
        if (string.IsNullOrEmpty(resourcePath))
        {
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (frameNames != null && frameNames.Length > 0)
        {
            LoadFrameFiles(8);
            return;
        }

        frames = RuntimeSpriteCropper.LoadNormalizedGridFrames(resourcePath, columns, rows, pixelsPerUnit, cropPadding, ignoreLastFrames, 8);
        ResetAnimation();
    }

    private void LoadFrameFiles(int bottomPadding)
    {
        if (string.IsNullOrEmpty(resourcePath) || frameNames == null || frameNames.Length == 0)
        {
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        frames = RuntimeSpriteCropper.LoadNormalizedFrameFiles(resourcePath, frameNames, pixelsPerUnit, cropPadding, bottomPadding);
        ResetAnimation();
    }

    private void ResetAnimation()
    {
        frameIndex = 0;
        frameDirection = 1;
        timer = 0f;
        if (spriteRenderer != null && frames.Length > 0)
        {
            spriteRenderer.sprite = frames[0];
            spriteRenderer.color = Color.white;
            SpriteLit.Apply(spriteRenderer);
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
            AdvanceFrame();
            spriteRenderer.sprite = frames[frameIndex];
        }
    }

    private void AdvanceFrame()
    {
        if (pingPong)
        {
            frameIndex += frameDirection;
            if (frameIndex >= frames.Length)
            {
                frameDirection = -1;
                frameIndex = Mathf.Max(0, frames.Length - 2);
            }
            else if (frameIndex < 0)
            {
                frameDirection = 1;
                frameIndex = loop ? Mathf.Min(1, frames.Length - 1) : 0;
            }

            return;
        }

        frameIndex++;
        if (frameIndex >= frames.Length)
        {
            frameIndex = loop ? 0 : frames.Length - 1;
        }
    }
}
