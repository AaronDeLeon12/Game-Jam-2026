using UnityEngine;

public enum TerrainType
{
    Floor,
    Wall,
    Obstacle,
    Platform
}

/// <summary>
/// Reusable level-building block. Self-configures its sprite, collider,
/// ground marker and one-way effector based on <see cref="TerrainType"/>.
/// Use it two ways:
///  - Scene: add this component to a GameObject, set Type/Size, right-click
///    the component header -> "Apply Terrain Settings" to preview in-editor.
///  - Code: TerrainBlock.Spawn(TerrainType.Floor, pos, size, parent).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class TerrainBlock : MonoBehaviour
{
    [SerializeField] private TerrainType type = TerrainType.Floor;
    [SerializeField] private Vector2 size = Vector2.one;
    [Tooltip("If alpha > 0 this overrides the default color for the type.")]
    [SerializeField] private Color colorOverride = new Color(0f, 0f, 0f, 0f);

    public TerrainType Type => type;

    private void Awake()
    {
        Apply();
    }

    public void Configure(TerrainType newType, Vector2 newSize)
    {
        type = newType;
        size = newSize;
        Apply();
    }

    [ContextMenu("Apply Terrain Settings")]
    public void Apply()
    {
        // Floor, Platform and Obstacle are all standable (their top counts as
        // ground so the player can jump off them). Only Wall is a pure barrier.
        // Platform vs Obstacle: Platform is one-way (pass through from below /
        // the sides), Obstacle is solid from every side.
        bool countsAsGround = type != TerrainType.Wall;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = PlaceholderSprites.Square;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.color = colorOverride.a > 0f ? colorOverride : DefaultColor(type);
        sr.sortingOrder = type == TerrainType.Floor ? 0 : 1;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.size = size;
        col.offset = Vector2.zero;

        GroundSurface2D ground = GetComponent<GroundSurface2D>();
        if (countsAsGround && ground == null)
        {
            gameObject.AddComponent<GroundSurface2D>();
        }
        else if (!countsAsGround && ground != null)
        {
            SafeDestroy(ground);
        }

        PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
        if (type == TerrainType.Platform)
        {
            col.usedByEffector = true;
            if (effector == null)
            {
                effector = gameObject.AddComponent<PlatformEffector2D>();
            }
            effector.useOneWay = true;
            effector.surfaceArc = 170f;
        }
        else
        {
            col.usedByEffector = false;
            if (effector != null)
            {
                SafeDestroy(effector);
            }
        }
    }

    public static TerrainBlock Spawn(TerrainType type, Vector2 position, Vector2 size, Transform parent = null, string name = null)
    {
        GameObject go = new GameObject(string.IsNullOrEmpty(name) ? type.ToString() : name);
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }

        go.transform.position = position;
        TerrainBlock block = go.AddComponent<TerrainBlock>();
        block.Configure(type, size);
        return block;
    }

    private static Color DefaultColor(TerrainType type)
    {
        switch (type)
        {
            case TerrainType.Wall:
                return new Color(0.5f, 0.42f, 0.35f);
            case TerrainType.Obstacle:
                return new Color(0.35f, 0.5f, 0.48f);
            case TerrainType.Platform:
                return new Color(0.42f, 0.45f, 0.5f);
            default:
                return new Color(0.28f, 0.28f, 0.32f);
        }
    }

    private static void SafeDestroy(Object target)
    {
        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}
