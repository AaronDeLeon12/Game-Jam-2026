using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Level content for the "home" scene. Ensures the day state and game
/// systems exist, then builds a base home plus per-day variations driven by
/// DayManager.CurrentDay (the home changes a little each day).
/// </summary>
public class HomeScene : MonoBehaviour
{
    private int day = 1;

    private void Awake()
    {
        HomeMode.IsActive = true;

        DayManager.EnsureExists();
        SystemsBootstrap.EnsureExists();

        day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;

        BuildSpawnPoint();
        BuildInteriorLight();
        // Base home layout (floor/walls) is now authored visually in the
        // home_day_1 scene. Only per-day changes are built in code.
        BuildDayVariations(day);
    }

    private void BuildInteriorLight()
    {
        GameObject lightObj = new GameObject("Home Interior Light");
        lightObj.transform.position = new Vector3(0f, -0.5f, 0f);

        Light2D light = lightObj.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        // Warm interior glow so the house reads bright vs the dark exterior.
        light.color = new Color(1f, 0.86f, 0.62f);
        light.intensity = 1.4f;
        light.pointLightInnerRadius = 3f;
        light.pointLightOuterRadius = 13f;
    }

    private void OnDestroy()
    {
        HomeMode.IsActive = false;
    }

    private void BuildSpawnPoint()
    {
        GameObject spawn = new GameObject("Player Spawn Point");
        spawn.transform.position = new Vector3(-2f, 0.5f, 0f);
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
