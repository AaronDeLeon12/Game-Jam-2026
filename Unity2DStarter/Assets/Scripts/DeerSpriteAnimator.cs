using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animates the ciervo (deer) NPC from the sheet at Resources/NPC/ciervo.
/// Same method the player animator uses: load a readable Texture2D, crop
/// manual frame rects, flood-fill the background to transparent, then cycle
/// the frames on a SpriteRenderer. Put this on the ciervo NPC GameObject.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DeerSpriteAnimator : MonoBehaviour
{
    [SerializeField] private string sheetResourcePath = "NPC/ciervo";
    [SerializeField] private float framesPerSecond = 6f;
    [SerializeField] private float targetWorldHeight = 4.8f;
    [Tooltip("Seconds the deer holds the head-up pose (frame 1) before bending down to eat.")]
    [SerializeField] private float idleHoldSeconds = 5f;

    // Top-left origin rects (sheet is 1364x1153, 4 cols x 2 rows). The bands
    // exclude the caption text under each frame; the background flood-fill +
    // visible-rect crop then tighten each frame to the deer art itself.
    private static readonly Rect[] FrameRects =
    {
        new Rect(0f, 20f, 341f, 460f),
        new Rect(341f, 20f, 341f, 460f),
        new Rect(682f, 20f, 341f, 460f),
        new Rect(1023f, 20f, 341f, 460f),
        new Rect(0f, 596f, 341f, 460f),
        new Rect(341f, 596f, 341f, 460f),
        new Rect(682f, 596f, 341f, 460f),
        new Rect(1023f, 596f, 341f, 460f)
    };

    private readonly List<Sprite> frames = new List<Sprite>();
    private SpriteRenderer spriteRenderer;
    private int[] sequence;
    private int seqPos;
    private float frameTimer;
    private bool isHolding = true;
    private float holdTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        LoadFrames();
        BuildSequence();

        if (frames.Count > 0)
        {
            ApplySprite(frames[0]);
            SpriteLit.Apply(spriteRenderer);
        }
    }

    // Only the first half of the frames are ever used: play them forwards
    // (going down) then the same frames backwards (coming up). For 8 frames
    // that is [0,1,2,3,2,1,0].
    private void BuildSequence()
    {
        if (frames.Count == 0)
        {
            sequence = new int[0];
            return;
        }

        int half = Mathf.Max(2, frames.Count / 2);
        half = Mathf.Min(half, frames.Count);

        List<int> seq = new List<int>();
        for (int i = 0; i < half; i++)
        {
            seq.Add(i);
        }

        for (int i = half - 2; i >= 0; i--)
        {
            seq.Add(i);
        }

        sequence = seq.ToArray();
    }

    private void Update()
    {
        if (frames.Count == 0 || sequence == null || sequence.Length == 0 || PauseMenu.IsPaused)
        {
            return;
        }

        // Cooldown: hold the head-up pose (first frame) for a while.
        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= idleHoldSeconds)
            {
                holdTimer = 0f;
                frameTimer = 0f;
                isHolding = false;
                seqPos = 0;
            }

            return;
        }

        // Ping-pong the first half: down (0..half-1) then back up (..0).
        frameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            seqPos++;

            if (seqPos >= sequence.Length)
            {
                // Back at the top: hold the head-up pose again.
                isHolding = true;
                holdTimer = 0f;
                seqPos = 0;
                ApplySprite(frames[sequence[0]]);
                return;
            }

            ApplySprite(frames[sequence[seqPos]]);
        }
    }

    private void LoadFrames()
    {
        frames.Clear();

        Texture2D sheet = Resources.Load<Texture2D>(sheetResourcePath);
        if (sheet == null)
        {
            Debug.LogWarning($"DeerSpriteAnimator: sheet not found at Resources/{sheetResourcePath}");
            return;
        }

        Color background = sheet.GetPixel(2, sheet.height - 2);
        foreach (Rect topLeft in FrameRects)
        {
            try
            {
                Sprite frame = CreateFrameFromSheet(sheet, topLeft, background);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            catch (UnityException)
            {
            }
        }
    }

    private void ApplySprite(Sprite sprite)
    {
        if (spriteRenderer == null || sprite == null)
        {
            return;
        }

        spriteRenderer.drawMode = SpriteDrawMode.Simple;
        spriteRenderer.sprite = sprite;

        float h = sprite.bounds.size.y;
        if (h > 0f)
        {
            float scale = targetWorldHeight / h;
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    // ---- Slicing helpers (same approach as PlayerSpriteAnimator) ----

    private static Sprite CreateFrameFromSheet(Texture2D sheet, Rect topLeftRect, Color background)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(topLeftRect.x), 0, sheet.width - 1);
        int yTop = Mathf.Clamp(Mathf.RoundToInt(topLeftRect.y), 0, sheet.height - 1);
        int width = Mathf.Clamp(Mathf.RoundToInt(topLeftRect.width), 1, sheet.width - x);
        int height = Mathf.Clamp(Mathf.RoundToInt(topLeftRect.height), 1, sheet.height - yTop);
        int y = Mathf.Clamp(sheet.height - yTop - height, 0, sheet.height - 1);

        Texture2D raw = new Texture2D(width, height, TextureFormat.RGBA32, false);
        raw.SetPixels(sheet.GetPixels(x, y, width, height));
        raw.Apply(false, false);

        Texture2D transparent = MakeBackgroundTransparent(raw, background);
        Rect crop = FindVisibleRect(transparent);
        Sprite sprite = Sprite.Create(transparent, crop, new Vector2(0.5f, 0.5f), Mathf.Max(1f, crop.height));
        sprite.name = "ciervo_frame";
        return sprite;
    }

    private static Texture2D MakeBackgroundTransparent(Texture2D source, Color background)
    {
        Color[] pixels = source.GetPixels();
        bool[] mask = FindEdgeBackgroundMask(source, background);
        for (int i = 0; i < pixels.Length; i++)
        {
            if (mask[i])
            {
                pixels[i] = Color.clear;
            }
        }

        Texture2D tex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        tex.SetPixels(pixels);
        tex.Apply(false, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    private static bool IsBackground(Color c, Color bg)
    {
        float distance = Mathf.Abs(c.r - bg.r) + Mathf.Abs(c.g - bg.g) + Mathf.Abs(c.b - bg.b);
        bool nearlyWhite = c.r > 0.92f && c.g > 0.92f && c.b > 0.92f;
        return distance < 0.16f || nearlyWhite;
    }

    private static bool[] FindEdgeBackgroundMask(Texture2D texture, Color background)
    {
        Color[] pixels = texture.GetPixels();
        bool[] visited = new bool[pixels.Length];
        Queue<int> queue = new Queue<int>();

        void TryEnqueue(int px, int py)
        {
            if (px < 0 || px >= texture.width || py < 0 || py >= texture.height)
            {
                return;
            }

            int index = py * texture.width + px;
            if (visited[index] || !IsBackground(pixels[index], background))
            {
                return;
            }

            visited[index] = true;
            queue.Enqueue(index);
        }

        for (int px = 0; px < texture.width; px++)
        {
            TryEnqueue(px, 0);
            TryEnqueue(px, texture.height - 1);
        }

        for (int py = 0; py < texture.height; py++)
        {
            TryEnqueue(0, py);
            TryEnqueue(texture.width - 1, py);
        }

        while (queue.Count > 0)
        {
            int index = queue.Dequeue();
            int cx = index % texture.width;
            int cy = index / texture.width;

            TryEnqueue(cx + 1, cy);
            TryEnqueue(cx - 1, cy);
            TryEnqueue(cx, cy + 1);
            TryEnqueue(cx, cy - 1);
        }

        return visited;
    }

    private static Rect FindVisibleRect(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        int minX = texture.width;
        int minY = texture.height;
        int maxX = 0;
        int maxY = 0;

        for (int py = 0; py < texture.height; py++)
        {
            for (int px = 0; px < texture.width; px++)
            {
                if (pixels[py * texture.width + px].a <= 0.08f)
                {
                    continue;
                }

                minX = Mathf.Min(minX, px);
                minY = Mathf.Min(minY, py);
                maxX = Mathf.Max(maxX, px);
                maxY = Mathf.Max(maxY, py);
            }
        }

        if (minX > maxX || minY > maxY)
        {
            return new Rect(0f, 0f, texture.width, texture.height);
        }

        const int pad = 8;
        minX = Mathf.Max(0, minX - pad);
        minY = Mathf.Max(0, minY - pad);
        maxX = Mathf.Min(texture.width - 1, maxX + pad);
        maxY = Mathf.Min(texture.height - 1, maxY + pad);

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}
