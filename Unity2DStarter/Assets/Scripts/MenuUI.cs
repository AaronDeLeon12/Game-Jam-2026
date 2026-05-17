using UnityEngine;

/// <summary>
/// Lightweight IMGUI helpers for prototype menus. This intentionally avoids
/// com.unity.ugui so the project can compile even if Unity's package cache
/// needs to rebuild.
/// </summary>
public static class MenuUI
{
    private static Font gameFont;

    public static Font GameFont
    {
        get
        {
            if (gameFont == null)
            {
                gameFont = Resources.Load<Font>("Fonts/CoralinesCat") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return gameFont;
        }
    }

    public static GUIStyle MakeLabelStyle(int fontSize)
    {
        return new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = fontSize,
            font = GameFont,
            normal = { textColor = Color.white }
        };
    }

    public static GUIStyle MakeButtonStyle(int fontSize)
    {
        return new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = fontSize,
            font = GameFont
        };
    }

    public static Rect CenteredRect(float y, float width, float height)
    {
        return new Rect((Screen.width - width) * 0.5f, y, width, height);
    }
}
