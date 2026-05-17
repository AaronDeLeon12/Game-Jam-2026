using UnityEngine;

/// <summary>
/// Drop-in invisible solid wall, used to delimit a scenario (left/right or
/// top boundaries the player must not cross). Just add this component to a
/// GameObject and it becomes a hidden barrier:
///  - Ensures a solid (non-trigger) BoxCollider2D so it blocks the player.
///  - Hides any SpriteRenderer (kept disabled, not destroyed) so nothing
///    shows in game while you can still see/size it in the editor.
///  - No GroundSurface2D, so it is a pure barrier (not standable), like a
///    TerrainType.Wall.
///
/// Use it two ways:
///  - Scene: add the component, set Size, right-click the header ->
///    "Apply Invisible Wall".
///  - Code: InvisibleWall.Spawn(position, size, parent).
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class InvisibleWall : MonoBehaviour
{
    [Tooltip("World-space size of the barrier (also sizes the collider).")]
    [SerializeField] private Vector2 size = new Vector2(1f, 10f);

    public Vector2 Size => size;

    private void Awake()
    {
        Apply();
    }

    public void Configure(Vector2 newSize)
    {
        size = newSize;
        Apply();
    }

    [ContextMenu("Apply Invisible Wall")]
    public void Apply()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = false;       // solid: it blocks the player
        col.usedByEffector = false;  // never one-way
        col.size = size;
        col.offset = Vector2.zero;

        // Hide the visual but keep it so the wall is still sizable in-editor.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }

        // A delimiting wall must not count as ground (not standable).
        GroundSurface2D ground = GetComponent<GroundSurface2D>();
        if (ground != null)
        {
            SafeDestroy(ground);
        }
    }

    public static InvisibleWall Spawn(Vector2 position, Vector2 size,
        Transform parent = null, string name = null)
    {
        GameObject go = new GameObject(string.IsNullOrEmpty(name) ? "Invisible Wall" : name);
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }

        go.transform.position = position;
        InvisibleWall wall = go.AddComponent<InvisibleWall>();
        wall.Configure(size);
        return wall;
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
