using System.Collections.Generic;
using UnityEngine;

public static class MagicAttackSprites
{
    private const string SheetPath = "Player/Attacks/attackCycles";
    private const string ShieldSheetPath = "Player/Shield/bubble_shield_cycle";
    private static Sprite[] squareFrames;
    private static Sprite[] triangleFrames;
    private static Sprite[] shieldFrames;

    public static Sprite[] SquareFrames => squareFrames ??= LoadAttackRow("Attack 3", new[]
    {
        new Rect(38f, 652f, 135f, 68f),
        new Rect(184f, 646f, 145f, 76f),
        new Rect(352f, 646f, 150f, 76f),
        new Rect(515f, 648f, 330f, 76f)
    });

    public static Sprite[] TriangleFrames => triangleFrames ??= LoadAttackRow("Attack 2", new[]
    {
        new Rect(37f, 742f, 125f, 90f),
        new Rect(183f, 741f, 160f, 82f),
        new Rect(352f, 733f, 170f, 95f),
        new Rect(515f, 733f, 170f, 95f),
        new Rect(690f, 735f, 270f, 95f)
    });

    public static Sprite[] ShieldFrames => shieldFrames ??= LoadBubbleShieldFrames();

    public static Sprite ShieldSprite => FirstOrFallback(ShieldFrames, null);

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

    private static Sprite[] LoadBubbleShieldFrames()
    {
        Texture2D source = Resources.Load<Texture2D>(ShieldSheetPath);
        if (source == null)
        {
            return new Sprite[0];
        }

        Rect[] topLeftRects =
        {
            new Rect(18f, 70f, 345f, 345f),
            new Rect(352f, 70f, 345f, 345f),
            new Rect(690f, 70f, 345f, 345f),
            new Rect(18f, 425f, 345f, 345f),
            new Rect(352f, 425f, 345f, 345f),
            new Rect(690f, 425f, 345f, 345f)
        };

        List<Sprite> frames = new List<Sprite>();
        Color background = source.GetPixel(2, source.height - 2);
        for (int i = 0; i < topLeftRects.Length; i++)
        {
            Rect rect = ToBottomLeftRect(source, topLeftRects[i]);
            Sprite sprite = CreateTransparentFrame(source, rect, background, $"Bubble Shield Frame {i + 1}", true);
            if (sprite != null)
            {
                frames.Add(sprite);
            }
        }

        return frames.ToArray();
    }

    private static Rect ToBottomLeftRect(Texture2D source, Rect topLeftRect)
    {
        return new Rect(
            topLeftRect.x,
            source.height - topLeftRect.y - topLeftRect.height,
            topLeftRect.width,
            topLeftRect.height);
    }

    private static Sprite CreateTransparentFrame(Texture2D source, Rect rect, Color background, string name, bool preserveSparkles = false)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(rect.x), 0, source.width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(rect.y), 0, source.height - 1);
        int width = Mathf.Clamp(Mathf.RoundToInt(rect.width), 1, source.width - x);
        int height = Mathf.Clamp(Mathf.RoundToInt(rect.height), 1, source.height - y);

        Texture2D frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = source.GetPixels(x, y, width, height);
        bool[] backgroundMask = FindEdgeBackgroundMask(pixels, width, height, background);
        for (int i = 0; i < pixels.Length; i++)
        {
            if (backgroundMask[i] || pixels[i].a < 0.08f)
            {
                pixels[i] = Color.clear;
            }
            else if (preserveSparkles && pixels[i].a < 0.75f)
            {
                pixels[i].a = Mathf.Max(pixels[i].a, 0.75f);
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

    private static bool[] FindEdgeBackgroundMask(Color[] pixels, int width, int height, Color background)
    {
        bool[] visited = new bool[pixels.Length];
        Queue<int> queue = new Queue<int>();

        void TryEnqueue(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            int index = y * width + x;
            if (visited[index] || !IsBackground(pixels[index], background))
            {
                return;
            }

            visited[index] = true;
            queue.Enqueue(index);
        }

        for (int x = 0; x < width; x++)
        {
            TryEnqueue(x, 0);
            TryEnqueue(x, height - 1);
        }

        for (int y = 0; y < height; y++)
        {
            TryEnqueue(0, y);
            TryEnqueue(width - 1, y);
        }

        while (queue.Count > 0)
        {
            int index = queue.Dequeue();
            int x = index % width;
            int y = index / width;

            TryEnqueue(x + 1, y);
            TryEnqueue(x - 1, y);
            TryEnqueue(x, y + 1);
            TryEnqueue(x, y - 1);
        }

        return visited;
    }

    private static bool IsBackground(Color color, Color background)
    {
        float distance = Mathf.Abs(color.r - background.r) + Mathf.Abs(color.g - background.g) + Mathf.Abs(color.b - background.b);
        bool nearlyPeach = color.r > 0.78f && color.g > 0.58f && color.b > 0.42f;
        return distance < 0.2f || nearlyPeach;
    }
}
