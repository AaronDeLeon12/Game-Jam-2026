using System.Collections.Generic;
using UnityEngine;

public static class RuntimeSpriteCropper
{
    public static Sprite LoadTrimmedSprite(string resourcePath, float pixelsPerUnit = 256f, int padding = 8)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        Texture2D transparent = MakeTransparent(texture);
        Rect crop = FindVisibleRect(transparent, padding);
        return Sprite.Create(transparent, crop, new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    public static Sprite[] LoadGridFrames(string resourcePath, int columns, int rows, float pixelsPerUnit = 256f, int padding = 6)
    {
        Texture2D sheet = Resources.Load<Texture2D>(resourcePath);
        if (sheet == null || columns <= 0 || rows <= 0)
        {
            return new Sprite[0];
        }

        List<Sprite> frames = new List<Sprite>();
        int frameWidth = sheet.width / columns;
        int frameHeight = sheet.height / rows;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int x = col * frameWidth;
                int y = sheet.height - (row + 1) * frameHeight;

                Texture2D frame = new Texture2D(frameWidth, frameHeight, TextureFormat.RGBA32, false);
                frame.SetPixels(sheet.GetPixels(x, y, frameWidth, frameHeight));
                frame.Apply(false, false);

                Texture2D transparent = MakeTransparent(frame);
                Rect crop = FindVisibleRect(transparent, padding);
                if (crop.width <= 4f || crop.height <= 4f)
                {
                    continue;
                }

                frames.Add(Sprite.Create(transparent, crop, new Vector2(0.5f, 0.5f), pixelsPerUnit));
            }
        }

        return frames.ToArray();
    }

    private static Texture2D MakeTransparent(Texture2D source)
    {
        Color background = source.GetPixel(2, source.height - 2);
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

        Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        texture.filterMode = FilterMode.Bilinear;
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

    private static bool IsBackground(Color color, Color background)
    {
        float distance = Mathf.Abs(color.r - background.r)
            + Mathf.Abs(color.g - background.g)
            + Mathf.Abs(color.b - background.b);
        bool nearlyWhite = color.r > 0.92f && color.g > 0.92f && color.b > 0.92f;
        return distance < 0.16f || nearlyWhite;
    }

    private static Rect FindVisibleRect(Texture2D texture, int padding)
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
                if (pixels[y * texture.width + x].a <= 0.08f)
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

        minX = Mathf.Max(0, minX - padding);
        minY = Mathf.Max(0, minY - padding);
        maxX = Mathf.Min(texture.width - 1, maxX + padding);
        maxY = Mathf.Min(texture.height - 1, maxY + padding);

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}
