using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class SceneLighting
{
    public static void ReplaceGlobalLight(string lightName, Color color, float intensity)
    {
        foreach (Light2D existing in Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None))
        {
            if (existing != null && existing.lightType == Light2D.LightType.Global)
            {
                Object.Destroy(existing.gameObject);
            }
        }

        GameObject lightObj = new GameObject(lightName);
        Light2D light = lightObj.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.color = color;
        light.intensity = intensity;
    }
}
