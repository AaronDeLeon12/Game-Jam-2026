using UnityEngine;

/// <summary>
/// Shared settings state (volume, fullscreen) persisted via PlayerPrefs and
/// applied globally. Used by both the main menu and the pause menu so the
/// behaviour stays identical everywhere.
/// </summary>
public static class GameSettings
{
    private const string VolumeKey = "MasterVolume";
    private const string FullscreenKey = "Fullscreen";

    public static float Volume { get; private set; } = 0.8f;
    public static bool Fullscreen { get; private set; }

    private static bool loaded;

    public static void Load()
    {
        if (loaded)
        {
            return;
        }

        Volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 0.8f));
        Fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        loaded = true;
        Apply();
    }

    public static void Apply()
    {
        AudioListener.volume = Volume;

        if (Fullscreen)
        {
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        }
    }

    public static void ChangeVolume(float delta)
    {
        Volume = Mathf.Clamp01(Mathf.Round((Volume + delta) * 10f) / 10f);
        AudioListener.volume = Volume;
        PlayerPrefs.SetFloat(VolumeKey, Volume);
        PlayerPrefs.Save();
    }

    public static void ToggleFullscreen()
    {
        SetFullscreen(!Fullscreen);
    }

    public static void SetWindowed()
    {
        if (Fullscreen)
        {
            SetFullscreen(false);
        }
    }

    private static void SetFullscreen(bool value)
    {
        Fullscreen = value;
        Apply();
        PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
