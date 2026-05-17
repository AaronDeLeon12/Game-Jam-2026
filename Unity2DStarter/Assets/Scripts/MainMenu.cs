using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu. Builds its UI from the shared MenuUI helpers and reuses the
/// shared SettingsPanel so settings behave identically to the pause menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    private const string FirstSceneName = "home_day_1";

    private bool showingSettings;

    private void Awake()
    {
        GameSettings.Load();

        EnsureCamera();
        SetupMusic();
    }

    private void OnGUI()
    {
        // Background drawing
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = new Color(0.06f, 0.07f, 0.1f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (showingSettings)
        {
            SettingsPanel.Draw(() => showingSettings = false);
            return;
        }

        // Title
        GUI.Label(MenuUI.CenteredRect(100f, 900f, 320f), "Tales of Ivory Moss", MenuUI.MakeLabelStyle(72));
        
        // Match the button text size of the Pause Menu
        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(35);
        
        // Save default color
        Color originalBgColor = GUI.backgroundColor;

        // --- Play Button : Rich Forest Green ---
        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f); 
        if (GUI.Button(MenuUI.CenteredRect(590f, 620f, 75f), "Play", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            OnPlay();
        }

        // --- Settings Button : Warm Gold ---
        GUI.backgroundColor = new Color(0.9f, 0.8f, 0.4f); 
        if (GUI.Button(MenuUI.CenteredRect(740f, 620f, 75f), "Settings", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            showingSettings = true;
        }

        // --- Quit Button : Soft Brick/Berry Red ---
        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f); 
        if (GUI.Button(MenuUI.CenteredRect(890f, 620f, 75f), "Quit", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            OnQuit();
        }

        // Restore original color
        GUI.backgroundColor = originalBgColor;
    }

    private void SetupMusic()
    {
        GameAudio.PlayMusic("MainMenu", 0.45f);
    }

    private static void EnsureCamera()
    {
        if (Camera.main != null)
        {
            return;
        }

        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.backgroundColor = new Color(0.07f, 0.08f, 0.11f);
        cam.orthographic = true;
        camObj.AddComponent<AudioListener>();
    }

    private void OnPlay()
    {
        DayManager.EnsureExists();
        DayManager.Instance.StartNewGame();
        SceneManager.LoadScene(FirstSceneName);
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}