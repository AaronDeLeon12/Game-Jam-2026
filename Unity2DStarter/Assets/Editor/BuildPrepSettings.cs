#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BuildPrepSettings
{
    static BuildPrepSettings()
    {
        Apply();
    }

    private static void Apply()
    {
        PlayerSettings.companyName = "Game Jam 2026";
        PlayerSettings.productName = "Tales of Ivory Moss";

        Texture2D logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/gameLogo.png");
        if (logo != null)
        {
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, new[] { logo });
        }
    }
}
#endif
