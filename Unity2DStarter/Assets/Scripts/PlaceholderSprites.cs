using UnityEngine;

public static class PlaceholderSprites
{
    private static Sprite squareSprite;

    public static Sprite Square
    {
        get
        {
            if (squareSprite == null)
            {
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.white);
                texture.filterMode = FilterMode.Point;
                texture.Apply();

                squareSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                squareSprite.name = "Runtime Placeholder Square";
            }

            return squareSprite;
        }
    }

    public static SpriteRenderer MakeSquare(GameObject owner, Color color, int sortingOrder = 0)
    {
        SpriteRenderer renderer = owner.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = owner.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = Square;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }
}
