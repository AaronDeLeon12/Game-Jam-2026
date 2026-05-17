using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Level content for the "home" scene. Ensures the day state and game
/// systems exist, then builds a base home plus per-day variations driven by
/// DayManager.CurrentDay (the home changes a little each day).
/// </summary>
public class HomeScene : MonoBehaviour
{
    [SerializeField] private float cameraOrthographicSize = 2.4f;
    [Tooltip("Lower = camera sits lower so more floor is visible (default offset.y is 2.5).")]
    [SerializeField] private float cameraOffsetY = 0.6f;

    private int day = 1;

    private void Awake()
    {
        HomeMode.IsActive = true;

        DayManager.EnsureExists();

        // Create the flat light before EnsureExists() so the systems' dim
        // global light is skipped (it only adds one when none exists).
        BuildFlatLight();

        SystemsBootstrap.EnsureExists();
        GameAudio.PlayMusic("MainMenuOrHouse", 0.4f);
        ApplyCameraZoom();

        day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;

        BuildSpawnPoint();
        HouseWoodKit.BuildStarterSet(transform);
        // Base home layout (floor/walls) is now authored visually in the
        // home_day_1 scene. Only per-day changes are built in code.
        BuildDayVariations(day);
    }

    private void ApplyCameraZoom()
    {
        Camera cam = SystemsBootstrap.Instance != null && SystemsBootstrap.Instance.GameCamera != null
            ? SystemsBootstrap.Instance.GameCamera
            : Camera.main;

        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = cameraOrthographicSize;

            CameraFollow2D follow = cam.GetComponent<CameraFollow2D>();
            if (follow != null)
            {
                follow.SetOffset(new Vector3(0f, cameraOffsetY, -10f));
            }
        }
    }

    // No mood lighting in the home: a full, neutral global light so sprites
    // render at their true colors (Sprite-Lit needs *some* light or it goes
    // black). Created before EnsureExists() would not matter here since this
    // runs after it, but the systems' dim global is skipped when a global
    // light already exists.
    private void BuildFlatLight()
    {
        SceneLighting.ReplaceGlobalLight("Home Flat Light", Color.white, 1f);
    }

    private void OnDestroy()
    {
        HomeMode.IsActive = false;
    }

    private void BuildSpawnPoint()
    {
        GameObject spawn = new GameObject("Player Spawn Point");
        spawn.transform.position = new Vector3(-2f, 1.75f, 0f);
        spawn.AddComponent<PlayerSpawnPoint>();
    }

    /// <summary>
    /// Hook for "the home changes a little bit day after day".
    /// Extend this with furniture / events / NPCs keyed off the day number.
    /// Placeholder: one extra crate appears for each day that has passed.
    /// </summary>
    private void BuildDayVariations(int currentDay)
    {
        int crates = Mathf.Clamp(currentDay - 1, 0, 6);
        for (int i = 0; i < crates; i++)
        {
            float x = -4f + i * 2f;
            TerrainBlock.Spawn(TerrainType.Obstacle, new Vector2(x, -2.25f), new Vector2(1f, 1f), transform, "Day Crate " + (i + 1));
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            normal = { textColor = Color.white }
        };

        GUI.Label(new Rect(Screen.width - 220f, 20f, 200f, 40f), "Day " + day, style);
    }
}
