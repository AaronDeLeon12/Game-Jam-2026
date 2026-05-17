using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu. Builds its UI from the shared MenuUI helpers and reuses the
/// shared SettingsPanel so settings behave identically to the pause menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    private const string GameSceneName = "StarterScene";

    private bool showingSettings;

    private void Awake()
    {
        GameSettings.Load();

        EnsureCamera();
        SetupMusic();
    }

    private void OnGUI()
    {
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

        GUI.Label(MenuUI.CenteredRect(80f, 900f, 120f), "TITLE", MenuUI.MakeLabelStyle(72));
        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(32);
        if (GUI.Button(MenuUI.CenteredRect(250f, 420f, 70f), "Play", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            OnPlay();
        }

        if (GUI.Button(MenuUI.CenteredRect(340f, 420f, 70f), "Settings", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            showingSettings = true;
        }

        if (GUI.Button(MenuUI.CenteredRect(430f, 420f, 70f), "Quit", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            OnQuit();
        }
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
        SceneManager.LoadScene(GameSceneName);
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
