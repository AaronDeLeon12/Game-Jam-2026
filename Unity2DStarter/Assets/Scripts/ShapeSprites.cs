using UnityEngine;

public static class ShapeSprites
{
    private static Sprite square;
    private static Sprite triangle;
    private static Sprite circle;

    public static Sprite Get(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Triangle:
                return Triangle;
            case SpellType.Circle:
                return Circle;
            default:
                return Square;
        }
    }

    public static Sprite Square => square ??= CreateSquare();
    public static Sprite Triangle => triangle ??= CreateTriangle();
    public static Sprite Circle => circle ??= CreateCircle();

    private static Sprite CreateSquare()
    {
        Texture2D texture = CreateTexture(32);

        for (int y = 4; y < 28; y++)
        {
            for (int x = 4; x < 28; x++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        return Finish(texture, "Square Spell Sprite");
    }

    private static Sprite CreateTriangle()
    {
        Texture2D texture = CreateTexture(32);

        for (int y = 5; y < 28; y++)
        {
            float widthPercent = (y - 5) / 23f;
            int halfWidth = Mathf.CeilToInt(widthPercent * 12f);

            for (int x = 16 - halfWidth; x <= 16 + halfWidth; x++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        return Finish(texture, "Triangle Spell Sprite");
    }

    private static Sprite CreateCircle()
    {
        Texture2D texture = CreateTexture(32);
        Vector2 center = new Vector2(15.5f, 15.5f);

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                if (Vector2.Distance(new Vector2(x, y), center) <= 12f)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }
        }

        return Finish(texture, "Circle Spell Sprite");
    }

    private static Texture2D CreateTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;

        Color clear = new Color(0f, 0f, 0f, 0f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        return texture;
    }

    private static Sprite Finish(Texture2D texture, string name)
    {
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
        sprite.name = name;
        return sprite;
    }
}
