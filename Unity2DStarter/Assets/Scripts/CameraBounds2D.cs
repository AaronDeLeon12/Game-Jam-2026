using UnityEngine;

/// <summary>
/// Defines the playable rectangle the camera is not allowed to look past, so
/// the edges of the scenario / floor can never be seen. CameraFollow2D reads
/// the currently active bounds and clamps itself to them: as the player walks
/// toward an edge the camera slows and locks (it "stops following" on that
/// axis) while the player keeps moving.
///
/// By default the rectangle is derived automatically from the InvisibleWall
/// objects in the scene: their inner faces become the limits (left/right
/// walls clamp X, a floor/ceiling wall clamps Y). You can also untick
/// auto-detect and set the rectangle by hand.
///
/// Just add this component to any GameObject in the scenario (one per scene).
/// </summary>
public class CameraBounds2D : MonoBehaviour
{
    [SerializeField] private bool autoFromInvisibleWalls = true;
    [SerializeField] private bool clampX = true;
    [SerializeField] private bool clampY = false;
    [Tooltip("Used when auto-detect is off (or finds nothing).")]
    [SerializeField] private Vector2 min = new Vector2(-20f, -6f);
    [SerializeField] private Vector2 max = new Vector2(20f, 10f);

    /// <summary>The bounds the persistent camera should currently obey.</summary>
    public static CameraBounds2D Active { get; private set; }

    public bool ClampX => clampX;
    public bool ClampY => clampY;
    public Vector2 Min => min;
    public Vector2 Max => max;

    private void OnEnable()
    {
        Active = this;
        if (autoFromInvisibleWalls)
        {
            RecalculateFromWalls();
        }
    }

    private void OnDisable()
    {
        if (Active == this)
        {
            Active = null;
        }
    }

    /// <summary>
    /// Builds the play rectangle from the InvisibleWall colliders: each wall
    /// pushes the limit inward on the side it sits relative to the area center
    /// (left walls raise minX, right walls lower maxX, etc.).
    /// </summary>
    [ContextMenu("Recalculate From Invisible Walls")]
    public void RecalculateFromWalls()
    {
        InvisibleWall[] walls = FindObjectsByType<InvisibleWall>(FindObjectsSortMode.None);
        if (walls == null || walls.Length == 0)
        {
            return;
        }

        // Rough center of everything the walls enclose.
        Bounds total = new Bounds();
        bool started = false;
        foreach (InvisibleWall w in walls)
        {
            Collider2D c = w.GetComponent<Collider2D>();
            if (c == null) continue;
            if (!started) { total = c.bounds; started = true; }
            else total.Encapsulate(c.bounds);
        }
        if (!started)
        {
            return;
        }

        Vector2 center = total.center;
        float left = float.NegativeInfinity;
        float right = float.PositiveInfinity;
        float bottom = float.NegativeInfinity;
        float top = float.PositiveInfinity;

        foreach (InvisibleWall w in walls)
        {
            Collider2D col = w.GetComponent<Collider2D>();
            if (col == null) continue;
            Bounds b = col.bounds;

            // A side wall is tall & thin -> use it for X. A floor/ceiling wall
            // is wide & short -> use it for Y. Compare aspect to decide.
            bool vertical = b.size.y >= b.size.x;
            if (vertical)
            {
                if (b.center.x <= center.x) left = Mathf.Max(left, b.max.x);
                else right = Mathf.Min(right, b.min.x);
            }
            else
            {
                if (b.center.y <= center.y) bottom = Mathf.Max(bottom, b.max.y);
                else top = Mathf.Min(top, b.min.y);
            }
        }

        if (!float.IsInfinity(left) && !float.IsInfinity(right) && right > left)
        {
            min.x = left;
            max.x = right;
        }
        if (!float.IsInfinity(bottom) && !float.IsInfinity(top) && top > bottom)
        {
            min.y = bottom;
            max.y = top;
        }
    }

    /// <summary>
    /// Clamps a desired camera position so the camera's visible rectangle
    /// stays inside the bounds. If the play area is smaller than the view on
    /// an axis the camera is centered (fully locked) on that axis.
    /// </summary>
    public Vector3 Clamp(Vector3 desired, Camera cam)
    {
        if (cam == null || !cam.orthographic)
        {
            return desired;
        }

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        if (clampX)
        {
            float lo = min.x + halfW;
            float hi = max.x - halfW;
            desired.x = lo > hi ? (min.x + max.x) * 0.5f : Mathf.Clamp(desired.x, lo, hi);
        }

        if (clampY)
        {
            float lo = min.y + halfH;
            float hi = max.y - halfH;
            desired.y = lo > hi ? (min.y + max.y) * 0.5f : Mathf.Clamp(desired.y, lo, hi);
        }

        return desired;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 c = new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, 0f);
        Vector3 s = new Vector3(max.x - min.x, max.y - min.y, 0.1f);
        Gizmos.DrawWireCube(c, s);
    }
}
