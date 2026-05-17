using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ResourceSheetAnimator2D : MonoBehaviour
{
    [SerializeField] private string resourcePath;
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 3;
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private bool loop = true;
    [SerializeField] private int ignoreLastFrames;
    [SerializeField] private float pixelsPerUnit = 256f;
    [SerializeField] private int cropPadding = 6;

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private float timer;
    private int frameIndex;

    public void Configure(string newResourcePath, int newColumns, int newRows, float fps, bool shouldLoop, int ignoredLastFrames = 0, float ppu = 256f, int padding = 6)
    {
        resourcePath = newResourcePath;
        columns = newColumns;
        rows = newRows;
        framesPerSecond = fps;
        loop = shouldLoop;
        ignoreLastFrames = Mathf.Max(0, ignoredLastFrames);
        pixelsPerUnit = ppu;
        cropPadding = padding;
        LoadFrames();
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
        Sprite[] loaded = RuntimeSpriteCropper.LoadFixedGridFrames(resourcePath, columns, rows, pixelsPerUnit);
        int count = Mathf.Max(0, loaded.Length - ignoreLastFrames);
        frames = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            frames[i] = loaded[i];
        }

        frameIndex = 0;
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
            frameIndex++;
            if (frameIndex >= frames.Length)
            {
                frameIndex = loop ? 0 : frames.Length - 1;
            }

            spriteRenderer.sprite = frames[frameIndex];
        }
    }
}
