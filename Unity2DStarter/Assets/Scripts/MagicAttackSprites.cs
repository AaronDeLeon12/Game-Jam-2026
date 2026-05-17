using System.Collections.Generic;
using UnityEngine;

public static class MagicAttackSprites
{
    private const string SheetPath = "Player/Attacks/attackCycles";
    private static Sprite[] squareFrames;
    private static Sprite[] triangleFrames;
    private static Sprite shieldSprite;

    public static Sprite[] SquareFrames => squareFrames ??= LoadAttackRow("Attack 3", new[]
    {
        new Rect(38f, 642f, 135f, 80f),
        new Rect(184f, 635f, 145f, 90f),
        new Rect(352f, 635f, 150f, 90f),
        new Rect(515f, 637f, 330f, 90f)
    });

    public static Sprite[] TriangleFrames => triangleFrames ??= LoadAttackRow("Attack 2", new[]
    {
        new Rect(37f, 742f, 125f, 90f),
        new Rect(183f, 741f, 160f, 82f),
        new Rect(352f, 733f, 170f, 95f),
        new Rect(515f, 733f, 170f, 95f),
        new Rect(690f, 735f, 270f, 95f)
    });

    public static Sprite ShieldSprite
    {
        get
        {
            if (shieldSprite != null)
            {
                return shieldSprite;
            }

            Sprite[] frames = LoadAttackRow("Attack 5", new[]
            {
                new Rect(36f, 425f, 135f, 125f)
            });

            shieldSprite = frames.Length > 0 ? frames[0] : null;
            return shieldSprite;
        }
    }

    public static bool HasSheet => Resources.Load<Texture2D>(SheetPath) != null;

    public static Sprite FirstOrFallback(Sprite[] frames, Sprite fallback)
    {
        return frames != null && frames.Length > 0 ? frames[0] : fallback;
    }

    private static Sprite[] LoadAttackRow(string name, Rect[] sourceRects)
    {
        Texture2D source = Resources.Load<Texture2D>(SheetPath);
        if (source == null)
        {
            return new Sprite[0];
        }

        List<Sprite> frames = new List<Sprite>();
        Color background = source.GetPixel(2, source.height - 2);
        for (int i = 0; i < sourceRects.Length; i++)
        {
            Sprite sprite = CreateTransparentFrame(source, sourceRects[i], background, $"{name} Frame {i + 1}");
            if (sprite != null)
            {
                frames.Add(sprite);
            }
        }

        return frames.ToArray();
    }

    private static Sprite CreateTransparentFrame(Texture2D source, Rect rect, Color background, string name)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(rect.x), 0, source.width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(rect.y), 0, source.height - 1);
        int width = Mathf.Clamp(Mathf.RoundToInt(rect.width), 1, source.width - x);
        int height = Mathf.Clamp(Mathf.RoundToInt(rect.height), 1, source.height - y);

        Texture2D frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = source.GetPixels(x, y, width, height);
        for (int i = 0; i < pixels.Length; i++)
        {
            Color color = pixels[i];
            float distance = Mathf.Abs(color.r - background.r) + Mathf.Abs(color.g - background.g) + Mathf.Abs(color.b - background.b);
            if (distance < 0.22f || color.a < 0.08f)
            {
                pixels[i] = Color.clear;
            }
        }

        frame.SetPixels(pixels);
        frame.Apply(false, false);
        frame.filterMode = FilterMode.Point;
        frame.wrapMode = TextureWrapMode.Clamp;

        Sprite sprite = Sprite.Create(frame, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), Mathf.Max(height, 1));
        sprite.name = name;
        return sprite;
    }
}
