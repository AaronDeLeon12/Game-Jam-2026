using UnityEngine;

/// <summary>
/// Makes the walkable surface (the BoxCollider2D) sit LOWER than the visible
/// floor sprite, so the player / oruga appear to walk partway "into" the
/// floor art instead of perched on its very top edge.
///
/// Put this on a floor object that has a SpriteRenderer + BoxCollider2D
/// (a plain art floor, NOT a TerrainBlock which manages its own collider).
/// surfaceDropFraction = how far down from the sprite's top the surface is
/// (0 = top of sprite, 0.5 = middle, 1 = bottom).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class LoweredFloorHitbox : MonoBehaviour
{
    [Range(0f, 0.95f)]
    [SerializeField] private float surfaceDropFraction = 0.45f;

    private void Start()
    {
        Apply();
    }

    [ContextMenu("Apply Lowered Hitbox")]
    public void Apply()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (sr == null || sr.sprite == null || box == null)
        {
            return;
        }

        Bounds b = sr.bounds; // world space
        float topY = b.max.y;
        float botY = b.min.y;
        float surfaceY = topY - b.size.y * surfaceDropFraction; // walkable top

        float worldHeight = surfaceY - botY;
        if (worldHeight <= 0.0001f)
        {
            return;
        }

        Vector3 scale = transform.lossyScale;
        float sx = Mathf.Max(0.0001f, Mathf.Abs(scale.x));
        float sy = Mathf.Max(0.0001f, Mathf.Abs(scale.y));

        box.size = new Vector2(b.size.x / sx, worldHeight / sy);

        float worldCenterY = (surfaceY + botY) * 0.5f;
        box.offset = new Vector2(
            (b.center.x - transform.position.x) / sx,
            (worldCenterY - transform.position.y) / sy);
    }
}
