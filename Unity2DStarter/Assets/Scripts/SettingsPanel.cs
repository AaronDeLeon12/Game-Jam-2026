using System;
using UnityEngine;

/// <summary>
/// Shared IMGUI settings drawer used by the main menu and pause menu.
/// </summary>
public static class SettingsPanel
{
    public static void Draw(Action onBack)
    {
        // Match Title size and position with Main Menu / Pause Menu
        GUI.Label(MenuUI.CenteredRect(100f, 1000f, 150f), "SETTINGS", MenuUI.MakeLabelStyle(60));

        // Make the Volume Label larger
        GUI.Label(MenuUI.CenteredRect(300f, 1000f, 150f),
            "Volume: " + Mathf.RoundToInt(GameSettings.Volume * 100f) + "%",
            MenuUI.MakeLabelStyle(40));

        // Match the button text size of the other menus
        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(35);
        
        // Save default color before we start tinting
        Color originalBgColor = GUI.backgroundColor;

        // --- - Volume Button : Soft Brick/Berry Red ---
        // Slightly narrower width (440f) to indicate it's a slider-type adjustment
        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
        if (GUI.Button(MenuUI.CenteredRect(570f, 440f, 75f), "- Volume", buttonStyle))
        {
            GameSettings.ChangeVolume(-0.1f);
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
        }

        // --- + Volume Button : Rich Forest Green ---
        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
        if (GUI.Button(MenuUI.CenteredRect(470f, 440f, 75f), "+ Volume", buttonStyle))
        {
            GameSettings.ChangeVolume(0.1f);
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
        }

        // --- Fullscreen Toggle : Slate Blue/Purple ---
        // Full width (620f) like the standard menu buttons
        GUI.backgroundColor = new Color(0.6f, 0.65f, 0.9f);
        string fullscreenLabel = "Fullscreen: " + (GameSettings.Fullscreen ? "On" : "Off");
        if (GUI.Button(MenuUI.CenteredRect(750f, 620f, 75f), fullscreenLabel, buttonStyle))
        {
            GameSettings.ToggleFullscreen();
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
        }

        // --- Minimize Button : Warm Gold ---
        GUI.backgroundColor = new Color(0.9f, 0.8f, 0.4f);
        if (GUI.Button(MenuUI.CenteredRect(900f, 620f, 75f), "Minimize (Windowed)", buttonStyle))
        {
            GameSettings.SetWindowed();
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
        }

        // --- Back Button : Soft Brick/Berry Red ---
        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
        if (GUI.Button(MenuUI.CenteredRect(1200f, 620f, 75f), "Back", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            onBack?.Invoke();
        }

        // Restore original background color
        GUI.backgroundColor = originalBgColor;
    }
}