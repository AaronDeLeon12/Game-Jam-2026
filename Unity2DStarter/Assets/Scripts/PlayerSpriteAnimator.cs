using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    [SerializeField] private string walkFramesResourcePath = "Player/Walk";
    [SerializeField] private string walkFramePrefix = "walk";
    [SerializeField] private int walkFrameCount = 8;
    [SerializeField] private float walkFramesPerSecond = 8f;
    [SerializeField] private float jumpFramesPerSecond = 8f;
    [SerializeField] private float crouchFramesPerSecond = 10f;
    [SerializeField] private float crouchWalkFramesPerSecond = 8f;
    [SerializeField] private float minimumWalkSpeed = 0.05f;
    [SerializeField] private float targetWorldHeight = 1.45f;
    [SerializeField] private float crouchVisualScaleMultiplier = 0.72f;
    [SerializeField] private float deathGroundOffset = -0.55f;

    private readonly List<Sprite> walkFrames = new List<Sprite>();
    private readonly List<Sprite> jumpFrames = new List<Sprite>();
    private readonly List<Sprite> crouchFrames = new List<Sprite>();
    private readonly List<Sprite> crouchWalkFrames = new List<Sprite>();
    private readonly List<Sprite> teleportVanishFrames = new List<Sprite>();
    private readonly List<Sprite> teleportAppearFrames = new List<Sprite>();
    private readonly List<Sprite> spellAttackFrames = new List<Sprite>();
    private readonly List<Sprite> deathFrames = new List<Sprite>();
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private PlayerMovement2D movement;
    private Sprite idleSprite;
    private float frameTimer;
    private int frameIndex;
    private bool wasWalking;
    private bool wasGrounded = true;
    private bool wasDucking;
    private float landingFrameUntil;
    private float jumpStartFrameUntil;
    private bool crouchIntroPlaying;
    private bool isPlayingExternalAnimation;
    private bool isDeadVisual;
    private int lastFacingDirection = 1;
    public bool HasLoadedSprites => walkFrames.Count > 0;

    private static readonly Rect[] CharacterSheetFrameRects =
    {
        new Rect(86f, 90f, 250f, 350f),
        new Rect(380f, 90f, 270f, 350f),
        new Rect(682f, 88f, 270f, 352f),
        new Rect(1000f, 90f, 270f, 352f),
        new Rect(82f, 455f, 245f, 335f),
        new Rect(382f, 450f, 270f, 340f),
        new Rect(690f, 455f, 260f, 335f),
        new Rect(1030f, 455f, 235f, 335f)
    };

    private static readonly Rect[] JumpSheetFrameRects =
    {
        new Rect(55f, 315f, 150f, 340f),
        new Rect(270f, 340f, 190f, 295f),
        new Rect(500f, 330f, 205f, 310f),
        new Rect(730f, 255f, 180f, 365f),
        new Rect(930f, 230f, 175f, 390f),
        new Rect(1140f, 320f, 180f, 340f),
        new Rect(1340f, 350f, 160f, 295f),
        new Rect(1570f, 315f, 150f, 340f)
    };

    private static readonly Rect[] CrouchSheetFrameRects =
    {
        new Rect(35f, 315f, 190f, 350f),
        new Rect(250f, 345f, 230f, 315f),
        new Rect(470f, 390f, 235f, 275f),
        new Rect(690f, 410f, 220f, 255f),
        new Rect(910f, 420f, 210f, 245f),
        new Rect(1130f, 340f, 230f, 325f),
        new Rect(1350f, 360f, 220f, 305f),
        new Rect(1580f, 315f, 190f, 350f)
    };

    private static readonly Rect[] CrouchWalkSheetFrameRects =
    {
        new Rect(30f, 388f, 180f, 292f),
        new Rect(190f, 383f, 200f, 297f),
        new Rect(365f, 368f, 200f, 312f),
        new Rect(550f, 383f, 200f, 297f),
        new Rect(735f, 368f, 180f, 312f),
        new Rect(925f, 383f, 200f, 297f),
        new Rect(1110f, 368f, 200f, 312f),
        new Rect(1295f, 383f, 200f, 297f)
    };

    private static readonly Rect[] TeleportVanishRects =
    {
        new Rect(25f, 115f, 210f, 250f),
        new Rect(235f, 105f, 265f, 285f),
        new Rect(765f, 115f, 240f, 260f)
    };

    private static readonly Rect[] TeleportAppearRects =
    {
        new Rect(45f, 555f, 185f, 205f),
        new Rect(240f, 530f, 265f, 285f),
        new Rect(760f, 535f, 245f, 265f)
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
        LoadStateFrames();
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

        if (jumpFrames.Count == 0 || crouchFrames.Count == 0 || crouchWalkFrames.Count == 0)
        {
            LoadStateFrames();
        }

        ApplyIdleFrame();
    }

    private void Update()
    {
        if (spriteRenderer == null || walkFrames.Count == 0 || PauseMenu.IsPaused)
        {
            return;
        }

        if (isPlayingExternalAnimation)
        {
            return;
        }

        if (movement == null)
        {
            movement = GetComponent<PlayerMovement2D>();
        }

        int facingDirection = movement != null ? movement.FacingDirection : (body.linearVelocity.x < 0f ? -1 : 1);
        spriteRenderer.flipX = facingDirection < 0;

        bool isGrounded = movement != null ? movement.IsGroundedNow : Mathf.Abs(body.linearVelocity.y) < 0.05f;
        bool isDucking = movement != null && movement.IsDucking;
        bool hasMoveInput = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f
            || Input.GetKey(KeyCode.A)
            || Input.GetKey(KeyCode.D)
            || Input.GetKey(KeyCode.LeftArrow)
            || Input.GetKey(KeyCode.RightArrow);
        bool isWalking = hasMoveInput || Mathf.Abs(body.linearVelocity.x) > minimumWalkSpeed;

        if (!wasGrounded && isGrounded)
        {
            landingFrameUntil = Time.time + 0.12f;
        }
        else if (wasGrounded && !isGrounded)
        {
            jumpStartFrameUntil = Time.time + 0.12f;
            frameTimer = 0f;
            frameIndex = 0;
        }

        if (!wasDucking && isDucking)
        {
            crouchIntroPlaying = true;
            frameTimer = 0f;
            frameIndex = 0;
        }

        wasGrounded = isGrounded;
        wasDucking = isDucking;

        if (Time.time < landingFrameUntil && jumpFrames.Count > 0)
        {
            ApplySprite(jumpFrames[jumpFrames.Count - 1]);
            return;
        }

        if (!isGrounded && jumpFrames.Count > 0)
        {
            UpdateJumpAnimation();
            return;
        }

        if (isDucking)
        {
            UpdateCrouchAnimation(isWalking);
            return;
        }

        if (!isWalking)
        {
            ApplyIdleFrame();
            return;
        }

        if (!wasWalking || facingDirection != lastFacingDirection)
        {
            frameTimer = 0f;
            frameIndex = Mathf.Min(1, walkFrames.Count - 1);
            spriteRenderer.sprite = walkFrames[frameIndex];
            NormalizeSpriteScale();
        }

        wasWalking = true;
        lastFacingDirection = facingDirection;

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, walkFramesPerSecond);
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % walkFrames.Count;
            ApplySprite(walkFrames[frameIndex]);
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

    private void LoadStateFrames()
    {
        jumpFrames.Clear();
        crouchFrames.Clear();
        crouchWalkFrames.Clear();
        teleportVanishFrames.Clear();
        teleportAppearFrames.Clear();
        spellAttackFrames.Clear();
        deathFrames.Clear();

        jumpFrames.AddRange(LoadSheetFrames("Player/States/NinaSaltando", JumpSheetFrameRects, "jump"));
        crouchFrames.AddRange(LoadSheetFrames("Player/States/NinaSeAgacha", CrouchSheetFrameRects, "crouch"));
        crouchWalkFrames.AddRange(LoadSheetFrames("Player/States/caminandoAgachada", CrouchWalkSheetFrameRects, "crouch_walk"));
        teleportVanishFrames.AddRange(LoadSheetFrames("Player/States/teleportCycle", TeleportVanishRects, "teleport_vanish"));
        teleportAppearFrames.AddRange(LoadSheetFrames("Player/States/teleportCycle", TeleportAppearRects, "teleport_appear"));
        spellAttackFrames.AddRange(RuntimeSpriteCropper.LoadNormalizedFrameFiles(
            "Player/States",
            new[] { "spell1_8", "spell2", "spell3", "spell4", "spell5", "spell6", "spell7", "spell1_8" },
            320f,
            8,
            12));
        deathFrames.AddRange(RuntimeSpriteCropper.LoadNormalizedFrameFiles(
            "Player/States",
            new[] { "death1", "death2", "death3", "death4", "death5", "death6", "death7" },
            320f,
            8,
            12));
    }

    private void ApplyIdleFrame()
    {
        if (spriteRenderer == null || idleSprite == null)
        {
            return;
        }

        frameTimer = 0f;
        frameIndex = 0;
        wasWalking = false;
        spriteRenderer.drawMode = SpriteDrawMode.Simple;
        ApplySprite(idleSprite);
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 10;
    }

    private void UpdateJumpAnimation()
    {
        if (Time.time < jumpStartFrameUntil)
        {
            ApplySprite(jumpFrames[0]);
            return;
        }

        int firstLoopFrame = Mathf.Min(2, jumpFrames.Count - 1);
        int loopCount = Mathf.Min(4, jumpFrames.Count - firstLoopFrame);
        if (loopCount <= 0)
        {
            ApplySprite(jumpFrames[0]);
            return;
        }

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, jumpFramesPerSecond);
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % loopCount;
        }

        ApplySprite(jumpFrames[firstLoopFrame + frameIndex]);
    }

    private void UpdateCrouchAnimation(bool isWalking)
    {
        if (crouchIntroPlaying && crouchFrames.Count > 0)
        {
            frameTimer += Time.deltaTime;
            float frameDuration = 1f / Mathf.Max(1f, crouchFramesPerSecond);
            while (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                frameIndex++;
            }

            int lastIntroFrame = Mathf.Min(3, crouchFrames.Count - 1);
            if (frameIndex >= lastIntroFrame)
            {
                frameIndex = lastIntroFrame;
                crouchIntroPlaying = false;
            }

            ApplySprite(crouchFrames[frameIndex], false, crouchVisualScaleMultiplier);
            return;
        }

        if (isWalking && crouchWalkFrames.Count > 0)
        {
            frameTimer += Time.deltaTime;
            float frameDuration = 1f / Mathf.Max(1f, crouchWalkFramesPerSecond);
            while (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                frameIndex = (frameIndex + 1) % crouchWalkFrames.Count;
            }

            ApplySprite(crouchWalkFrames[frameIndex], false, crouchVisualScaleMultiplier);
            return;
        }

        if (crouchFrames.Count > 0)
        {
            ApplySprite(crouchFrames[Mathf.Min(3, crouchFrames.Count - 1)], false, crouchVisualScaleMultiplier);
        }
    }

    public IEnumerator PlayTeleportVanish(float duration)
    {
        yield return PlayExternalAnimation(teleportVanishFrames, duration);
    }

    public IEnumerator PlayTeleportAppear(float duration)
    {
        yield return PlayExternalAnimation(teleportAppearFrames, duration);
    }

    public void PlaySpellAttack(float duration = 0.28f)
    {
        if (spellAttackFrames.Count == 0 || isDeadVisual)
        {
            return;
        }

        StopCoroutine(nameof(PlayOneShotExternalAnimation));
        StartCoroutine(PlayOneShotExternalAnimation(spellAttackFrames, duration));
    }

    public void PlayDeath(float duration = 0.7f)
    {
        if (deathFrames.Count == 0 || isDeadVisual)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(PlayDeathAnimation(duration));
    }

    private IEnumerator PlayExternalAnimation(List<Sprite> frames, float duration)
    {
        if (frames.Count == 0)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }

        isPlayingExternalAnimation = true;

        int[] sequence = frames.Count >= 3
            ? new[] { 0, 1, 0, 1, 2 }
            : new[] { 0, 1, 0, 1 };
        float frameDuration = duration / sequence.Length;

        for (int i = 0; i < sequence.Length; i++)
        {
            ApplySprite(frames[Mathf.Clamp(sequence[i], 0, frames.Count - 1)], true);
            yield return new WaitForSeconds(frameDuration);
        }

        isPlayingExternalAnimation = false;
    }

    private IEnumerator PlayOneShotExternalAnimation(List<Sprite> frames, float duration)
    {
        if (frames.Count == 0)
        {
            yield break;
        }

        isPlayingExternalAnimation = true;
        float frameDuration = duration / Mathf.Max(1, frames.Count);
        for (int i = 0; i < frames.Count; i++)
        {
            ApplySpriteAtStandingScale(frames[i]);
            yield return new WaitForSecondsRealtime(frameDuration);
        }

        isPlayingExternalAnimation = false;
        ForceRefresh();
    }

    private IEnumerator PlayDeathAnimation(float duration)
    {
        isDeadVisual = true;
        isPlayingExternalAnimation = true;
        float frameDuration = duration / Mathf.Max(1, deathFrames.Count);
        for (int i = 0; i < deathFrames.Count; i++)
        {
            ApplySpriteAtStandingScale(deathFrames[i], deathGroundOffset);
            yield return new WaitForSecondsRealtime(frameDuration);
        }

        if (deathFrames.Count > 0)
        {
            ApplySpriteAtStandingScale(deathFrames[deathFrames.Count - 1], deathGroundOffset);
        }
    }

    private void ApplySpriteAtStandingScale(Sprite sprite, float localYOffset = 0f)
    {
        if (spriteRenderer == null || sprite == null)
        {
            return;
        }

        spriteRenderer.transform.localPosition = new Vector3(0f, localYOffset, 0f);
        spriteRenderer.sprite = sprite;

        if (idleSprite != null && idleSprite.bounds.size.y > 0f)
        {
            float scale = targetWorldHeight / idleSprite.bounds.size.y;
            spriteRenderer.transform.localScale = new Vector3(scale, scale, 1f);
        }
        else
        {
            NormalizeSpriteScale();
        }
    }

    private void ApplySprite(Sprite sprite, bool normalizeHeight = true, float relativeScaleMultiplier = 1f, float localYOffset = 0f)
    {
        if (spriteRenderer == null || sprite == null)
        {
            return;
        }

        spriteRenderer.transform.localPosition = new Vector3(0f, localYOffset, 0f);
        spriteRenderer.sprite = sprite;
        if (normalizeHeight)
        {
            NormalizeSpriteScale();
        }
        else
        {
            ApplyScaleRelativeToStanding(sprite, relativeScaleMultiplier);
        }
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

    private void ApplyScaleRelativeToStanding(Sprite sprite, float relativeScaleMultiplier)
    {
        if (sprite == null || idleSprite == null || idleSprite.bounds.size.y <= 0f)
        {
            NormalizeSpriteScale();
            return;
        }

        float standingScale = targetWorldHeight / idleSprite.bounds.size.y;
        float relativeHeight = sprite.rect.height / Mathf.Max(1f, idleSprite.rect.height);
        float scale = standingScale * relativeHeight * relativeScaleMultiplier;
        spriteRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private static Sprite CreateCroppedSprite(Texture2D texture, string spriteName)
    {
        try
        {
            Color background = texture.GetPixel(2, texture.height - 2);
            Texture2D transparentTexture = CreateTransparentTexture(texture, background);
            Rect crop = FindVisibleRect(transparentTexture);
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
        Texture2D sheet = Resources.Load<Texture2D>("Player/personajePrincipalClean");
        if (sheet == null)
        {
            sheet = Resources.Load<Texture2D>("Player/personajePrincipal");
        }
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

        Texture2D transparentFrame = CreateTransparentTexture(rawFrame, background);
        Rect crop = FindVisibleRect(transparentFrame);
        Sprite sprite = Sprite.Create(transparentFrame, crop, new Vector2(0.5f, 0.5f), crop.height);
        sprite.name = spriteName;
        return sprite;
    }

    private static Sprite[] LoadSheetFrames(string resourcePath, Rect[] topLeftRects, string spritePrefix)
    {
        Texture2D sheet = Resources.Load<Texture2D>(resourcePath);
        if (sheet == null)
        {
            return new Sprite[0];
        }

        List<Sprite> frames = new List<Sprite>();
        for (int i = 0; i < topLeftRects.Length; i++)
        {
            try
            {
                frames.Add(CreateFrameFromSheet(sheet, topLeftRects[i], $"{spritePrefix}_{i + 1}"));
            }
            catch (UnityException)
            {
            }
        }

        return frames.ToArray();
    }

    private static Texture2D CreateTransparentTexture(Texture2D source, Color background)
    {
        Color[] pixels = source.GetPixels();
        if (HasExistingTransparency(pixels))
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a <= 0.08f)
                {
                    pixels[i] = Color.clear;
                }
            }
        }
        else
        {
            bool[] backgroundMask = FindEdgeBackgroundMask(source, background);
            for (int i = 0; i < pixels.Length; i++)
            {
                if (backgroundMask[i])
                {
                    pixels[i] = Color.clear;
                }
            }
        }

        RemoveSmallVisibleIslands(pixels, source.width, source.height, 24);

        Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    private static bool HasExistingTransparency(Color[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a < 0.95f)
            {
                return true;
            }
        }

        return false;
    }

    private static Rect FindVisibleRect(Texture2D texture, Color background)
    {
        Color[] pixels = texture.GetPixels();
        bool[] backgroundMask = FindEdgeBackgroundMask(texture, background);
        int minX = texture.width;
        int minY = texture.height;
        int maxX = 0;
        int maxY = 0;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                int index = y * texture.width + x;
                Color pixel = pixels[index];
                if (pixel.a <= 0.08f || backgroundMask[index])
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

        const int horizontalPadding = 8;
        const int topPadding = 8;
        const int bottomPadding = 16;
        minX = Mathf.Max(0, minX - horizontalPadding);
        minY = Mathf.Max(0, minY - bottomPadding);
        maxX = Mathf.Min(texture.width - 1, maxX + horizontalPadding);
        maxY = Mathf.Min(texture.height - 1, maxY + topPadding);

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private static Rect FindVisibleRect(Texture2D texture)
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
                if (pixel.a <= 0.08f)
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

        const int horizontalPadding = 8;
        const int topPadding = 8;
        const int bottomPadding = 16;
        minX = Mathf.Max(0, minX - horizontalPadding);
        minY = Mathf.Max(0, minY - bottomPadding);
        maxX = Mathf.Min(texture.width - 1, maxX + horizontalPadding);
        maxY = Mathf.Min(texture.height - 1, maxY + topPadding);

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private static bool IsBackground(Color color, Color background)
    {
        float distance = Mathf.Abs(color.r - background.r) + Mathf.Abs(color.g - background.g) + Mathf.Abs(color.b - background.b);
        bool nearlyWhite = color.r > 0.92f && color.g > 0.92f && color.b > 0.92f;
        return distance < 0.16f || nearlyWhite;
    }

    private static bool[] FindEdgeBackgroundMask(Texture2D texture, Color background)
    {
        Color[] pixels = texture.GetPixels();
        bool[] visited = new bool[pixels.Length];
        Queue<int> queue = new Queue<int>();

        void TryEnqueue(int x, int y)
        {
            if (x < 0 || x >= texture.width || y < 0 || y >= texture.height)
            {
                return;
            }

            int index = y * texture.width + x;
            if (visited[index] || !IsBackground(pixels[index], background))
            {
                return;
            }

            visited[index] = true;
            queue.Enqueue(index);
        }

        for (int x = 0; x < texture.width; x++)
        {
            TryEnqueue(x, 0);
            TryEnqueue(x, texture.height - 1);
        }

        for (int y = 0; y < texture.height; y++)
        {
            TryEnqueue(0, y);
            TryEnqueue(texture.width - 1, y);
        }

        while (queue.Count > 0)
        {
            int index = queue.Dequeue();
            int x = index % texture.width;
            int y = index / texture.width;

            TryEnqueue(x + 1, y);
            TryEnqueue(x - 1, y);
            TryEnqueue(x, y + 1);
            TryEnqueue(x, y - 1);
        }

        return visited;
    }

    private static void RemoveSmallVisibleIslands(Color[] pixels, int width, int height, int minArea)
    {
        bool[] visited = new bool[pixels.Length];
        Queue<int> queue = new Queue<int>();
        List<int> component = new List<int>();

        for (int start = 0; start < pixels.Length; start++)
        {
            if (visited[start] || pixels[start].a <= 0.08f)
            {
                continue;
            }

            visited[start] = true;
            queue.Enqueue(start);
            component.Clear();

            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                component.Add(index);
                int x = index % width;
                int y = index / width;

                TryVisit(x + 1, y);
                TryVisit(x - 1, y);
                TryVisit(x, y + 1);
                TryVisit(x, y - 1);
            }

            if (component.Count < minArea)
            {
                for (int i = 0; i < component.Count; i++)
                {
                    pixels[component[i]] = Color.clear;
                }
            }
        }

        void TryVisit(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            int index = y * width + x;
            if (visited[index] || pixels[index].a <= 0.08f)
            {
                return;
            }

            visited[index] = true;
            queue.Enqueue(index);
        }
    }
}
