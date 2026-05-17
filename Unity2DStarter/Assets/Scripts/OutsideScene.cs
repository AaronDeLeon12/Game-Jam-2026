using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Level content for "outside_1". Ensures day state and game systems exist,
/// places a spawn point and a basic ground so the area is playable.
/// (HomeMode is automatically cleared when leaving the home scene, so the
/// HUD and full player abilities return here.)
/// </summary>
public class OutsideScene : MonoBehaviour
{
    private void Awake()
    {
        DayManager.EnsureExists();

        // Bright neutral daytime light. Created before EnsureExists() so the
        // systems' dim default global light is skipped (it only adds one when
        // no global light already exists), avoiding two lights stacking.
        BuildDaylight();

        SystemsBootstrap.EnsureExists();

        GameObject spawn = new GameObject("Player Spawn Point");
        spawn.transform.position = new Vector3(-8f, 0.5f, 0f);
        spawn.AddComponent<PlayerSpawnPoint>();

        // Floor is now a hand-placed scene object ("Floor" with TerrainBlock)
        // in outside_1, so it is not generated here anymore.
    }

    private void BuildDaylight()
    {
        GameObject lightObj = new GameObject("Daylight Global Light");

        Light2D light = lightObj.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.color = Color.white;
        light.intensity = 1f;
    }
}

