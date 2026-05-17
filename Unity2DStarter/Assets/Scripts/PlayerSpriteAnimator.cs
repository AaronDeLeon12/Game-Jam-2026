using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    [SerializeField] private string walkFramesResourcePath = "Player/Walk";
    [SerializeField] private string walkFramePrefix = "walk";
    [SerializeField] private int walkFrameCount = 8;
    [SerializeField] private float walkFramesPerSecond = 8f;
    [SerializeField] private float minimumWalkSpeed = 0.05f;
    [SerializeField] private float targetWorldHeight = 1.45f;

    private readonly List<Sprite> walkFrames = new List<Sprite>();
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private PlayerMovement2D movement;
    private Sprite idleSprite;
    private float frameTimer;
    private int frameIndex;
    public bool HasLoadedSprites => walkFrames.Count > 0;

    private static readonly Rect[] CharacterSheetFrameRects =
    {
        new Rect(105f, 125f, 220f, 395f),
        new Rect(410f, 120f, 260f, 405f),
        new Rect(735f, 118f, 265f, 410f),
        new Rect(1058f, 120f, 270f, 410f),
        new Rect(95f, 512f, 260f, 405f),
        new Rect(410f, 505f, 270f, 420f),
        new Rect(735f, 512f, 275f, 410f),
        new Rect(1065f, 505f, 250f, 420f)
    };

    public static Sprite LoadWalkSprite(int frameNumber)
    {
        Sprite sheetFrame = LoadCharacterSheetFrame(frameNumber);
        if (sheetFrame != null)
        {
            return sheetFrame;
        }

        Texture2D texture = Resources.Load<Texture2D>($"Player/Walk/walk{frameNumber}");
        if (texture != null)
        {
            return CreateCroppedSprite(texture, $"walk{frameNumber}");
        }

        return Resources.Load<Sprite>($"Player/Walk/walk{frameNumber}");
    }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement2D>();

        LoadWalkFrames();
        ApplyIdleFrame();
        NormalizeSpriteScale();
    }

    private void OnEnable()
    {
        ForceRefresh();
    }

    private void Start()
    {
        ForceRefresh();
    }

    public void ForceRefresh()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (walkFrames.Count == 0)
        {
            LoadWalkFrames();
        }

        ApplyIdleFrame();
    }

    private void Update()
    {
        if (spriteRenderer == null || walkFrames.Count == 0 || PauseMenu.IsPaused)
        {
            return;
        }

        if (movement == null)
        {
            movement = GetComponent<PlayerMovement2D>();
        }

        int facingDirection = movement != null ? movement.FacingDirection : (body.linearVelocity.x < 0f ? -1 : 1);
        spriteRenderer.flipX = facingDirection < 0;

        bool isWalking = Mathf.Abs(body.linearVelocity.x) > minimumWalkSpeed;
        if (!isWalking)
        {
            ApplyIdleFrame();
            return;
        }

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, walkFramesPerSecond);
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % walkFrames.Count;
            spriteRenderer.sprite = walkFrames[frameIndex];
            NormalizeSpriteScale();
        }
    }

    private void LoadWalkFrames()
    {
        walkFrames.Clear();

        for (int i = 1; i <= Mathf.Min(walkFrameCount, CharacterSheetFrameRects.Length); i++)
        {
            Sprite frame = LoadWalkSprite(i);
            if (frame == null)
            {
                frame = Resources.Load<Sprite>($"{walkFramesResourcePath}/{walkFramePrefix}{i}");
            }

            if (frame != null)
            {
                walkFrames.Add(frame);
            }
        }

        if (walkFrames.Count == 0)
        {
            return;
        }

        idleSprite = walkFrames[0];
    }

    private void ApplyIdleFrame()
    {
        if (spriteRenderer == null || idleSprite == null)
        {
            return;
        }

        frameTimer = 0f;
        frameIndex = 0;
        spriteRenderer.drawMode = SpriteDrawMode.Simple;
        spriteRenderer.sprite = idleSprite;
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 10;
        NormalizeSpriteScale();
    }

    private void NormalizeSpriteScale()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        float spriteHeight = spriteRenderer.sprite.bounds.size.y;
        if (spriteHeight <= 0f)
        {
            return;
        }

        float scale = targetWorldHeight / spriteHeight;
        spriteRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private static Sprite CreateCroppedSprite(Texture2D texture, string spriteName)
    {
        try
        {
            Color background = texture.GetPixel(2, texture.height - 2);
            Rect crop = FindVisibleRect(texture, background);
            Texture2D transparentTexture = CreateTransparentTexture(texture, background);
            Sprite sprite = Sprite.Create(transparentTexture, crop, new Vector2(0.5f, 0.5f), crop.height);
            sprite.name = spriteName;
            return sprite;
        }
        catch (UnityException)
        {
            return null;
        }
    }

    private static Sprite LoadCharacterSheetFrame(int frameNumber)
    {
        Texture2D sheet = Resources.Load<Texture2D>("Player/personajePrincipal");
        if (sheet == null || frameNumber < 1 || frameNumber > CharacterSheetFrameRects.Length)
        {
            return null;
        }

        try
        {
            return CreateFrameFromSheet(sheet, CharacterSheetFrameRects[frameNumber - 1], $"personaje_walk{frameNumber}");
        }
        catch (UnityException)
        {
            return null;
        }
    }

    private static Sprite CreateFrameFromSheet(Texture2D sheet, Rect topLeftRect, string spriteName)
    {
        Color background = sheet.GetPixel(2, sheet.height - 2);
        int x = Mathf.Clamp(Mathf.RoundToInt(topLeftRect.x), 0, sheet.width - 1);
        int yTop = Mathf.Clamp(Mathf.RoundToInt(topLeftRect.y), 0, sheet.height - 1);
        int width = Mathf.Clamp(Mathf.RoundToInt(topLeftRect.width), 1, sheet.width - x);
        int height = Mathf.Clamp(Mathf.RoundToInt(topLeftRect.height), 1, sheet.height - yTop);
        int y = Mathf.Clamp(sheet.height - yTop - height, 0, sheet.height - 1);

        Texture2D rawFrame = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = sheet.GetPixels(x, y, width, height);
        rawFrame.SetPixels(pixels);
        rawFrame.Apply(false, false);

        Rect crop = FindVisibleRect(rawFrame, background);
        Texture2D transparentFrame = CreateTransparentTexture(rawFrame, background);
        Sprite sprite = Sprite.Create(transparentFrame, crop, new Vector2(0.5f, 0.5f), crop.height);
        sprite.name = spriteName;
        return sprite;
    }

    private static Texture2D CreateTransparentTexture(Texture2D source, Color background)
    {
        Color[] pixels = source.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (IsBackground(pixels[i], background))
            {
                pixels[i] = Color.clear;
            }
        }

        Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    private static Rect FindVisibleRect(Texture2D texture, Color background)
    {
        Color[] pixels = texture.GetPixels();
        int minX = texture.width;
        int minY = texture.height;
        int maxX = 0;
        int maxY = 0;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color pixel = pixels[y * texture.width + x];
                if (pixel.a <= 0.08f || IsBackground(pixel, background))
                {
                    continue;
                }

                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
        }

        if (minX > maxX || minY > maxY)
        {
            return new Rect(0f, 0f, texture.width, texture.height);
        }

        const int padding = 8;
        minX = Mathf.Max(0, minX - padding);
        minY = Mathf.Max(0, minY - padding);
        maxX = Mathf.Min(texture.width - 1, maxX + padding);
        maxY = Mathf.Min(texture.height - 1, maxY + padding);

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private static bool IsBackground(Color color, Color background)
    {
        float distance = Mathf.Abs(color.r - background.r) + Mathf.Abs(color.g - background.g) + Mathf.Abs(color.b - background.b);
        bool nearlyWhite = color.r > 0.92f && color.g > 0.92f && color.b > 0.92f;
        return distance < 0.18f || nearlyWhite;
    }
}
