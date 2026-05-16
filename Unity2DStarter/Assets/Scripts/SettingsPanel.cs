using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable settings sub-panel (volume, fullscreen, minimize/windowed, back).
/// Built into any parent canvas; shared by the main menu and pause menu.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    private Text volumeLabel;
    private Text fullscreenLabel;
    private Action onBack;

    public static SettingsPanel Create(Transform parent, Action onBack)
    {
        GameObject go = MenuUI.CreateStretch(parent, "Settings Panel");
        SettingsPanel panel = go.AddComponent<SettingsPanel>();
        panel.onBack = onBack;
        panel.Build();
        return panel;
    }

    private void Build()
    {
        MenuUI.CreateLabel(transform, "SETTINGS", new Vector2(0f, 380f), new Vector2(1200f, 200f), 90);

        volumeLabel = MenuUI.CreateLabel(transform, "", new Vector2(0f, 200f), new Vector2(800f, 90f), 48);
        MenuUI.CreateButton(transform, "- Volume", new Vector2(-220f, 80f), () => { GameSettings.ChangeVolume(-0.1f); Refresh(); }, 360f);
        MenuUI.CreateButton(transform, "+ Volume", new Vector2(220f, 80f), () => { GameSettings.ChangeVolume(0.1f); Refresh(); }, 360f);

        fullscreenLabel = MenuUI.CreateButton(transform, "", new Vector2(0f, -60f), () => { GameSettings.ToggleFullscreen(); Refresh(); }, 560f);
        MenuUI.CreateButton(transform, "Minimize (Windowed)", new Vector2(0f, -190f), () => { GameSettings.SetWindowed(); Refresh(); }, 560f);
        MenuUI.CreateButton(transform, "Back", new Vector2(0f, -320f), () => onBack?.Invoke());

        Refresh();
    }

    public void Refresh()
    {
        if (volumeLabel != null)
        {
            volumeLabel.text = "Volume: " + Mathf.RoundToInt(GameSettings.Volume * 100f) + "%";
        }

        if (fullscreenLabel != null)
        {
            fullscreenLabel.text = "Fullscreen: " + (GameSettings.Fullscreen ? "On" : "Off");
        }
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
        if (show)
        {
            Refresh();
        }
    }
}
