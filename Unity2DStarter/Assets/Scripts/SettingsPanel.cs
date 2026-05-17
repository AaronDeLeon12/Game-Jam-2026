using System;
using UnityEngine;

/// <summary>
/// Shared IMGUI settings drawer used by the main menu and pause menu.
/// </summary>
public static class SettingsPanel
{
    public static void Draw(Action onBack)
    {
        GUI.Label(MenuUI.CenteredRect(70f, 900f, 90f), "SETTINGS", MenuUI.MakeLabelStyle(54));

        GUI.Label(MenuUI.CenteredRect(180f, 700f, 60f),
            "Volume: " + Mathf.RoundToInt(GameSettings.Volume * 100f) + "%",
            MenuUI.MakeLabelStyle(30));

        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(28);
        if (GUI.Button(MenuUI.CenteredRect(260f, 260f, 58f), "- Volume", buttonStyle))
        {
            GameSettings.ChangeVolume(-0.1f);
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
        }

        if (GUI.Button(MenuUI.CenteredRect(330f, 260f, 58f), "+ Volume", buttonStyle))
        {
            GameSettings.ChangeVolume(0.1f);
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
        }

        string fullscreenLabel = "Fullscreen: " + (GameSettings.Fullscreen ? "On" : "Off");
        if (GUI.Button(MenuUI.CenteredRect(420f, 420f, 58f), fullscreenLabel, buttonStyle))
        {
            GameSettings.ToggleFullscreen();
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
        }

        if (GUI.Button(MenuUI.CenteredRect(490f, 420f, 58f), "Minimize (Windowed)", buttonStyle))
        {
            GameSettings.SetWindowed();
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
        }

        if (GUI.Button(MenuUI.CenteredRect(590f, 360f, 62f), "Back", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            onBack?.Invoke();
        }
    }
}
