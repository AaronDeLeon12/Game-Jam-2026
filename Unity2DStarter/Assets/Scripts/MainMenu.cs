using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu. Builds its UI from the shared MenuUI helpers and reuses the
/// shared SettingsPanel so settings behave identically to the pause menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    private const string GameSceneName = "StarterScene";

    private GameObject mainPanel;
    private SettingsPanel settings;

    private void Awake()
    {
        GameSettings.Load();

        MenuUI.EnsureEventSystem();
        EnsureCamera();
        BuildUI();
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

    private void BuildUI()
    {
        Canvas canvas = MenuUI.CreateCanvas("Menu Canvas", 100);
        MenuUI.CreateStretchPanel(canvas.transform, "Background", new Color(0.06f, 0.07f, 0.1f, 1f));

        mainPanel = MenuUI.CreateStretch(canvas.transform, "Main Panel");
        MenuUI.CreateLabel(mainPanel.transform, "TITLE", new Vector2(0f, 360f), new Vector2(1200f, 200f), 110);
        MenuUI.CreateButton(mainPanel.transform, "Play", new Vector2(0f, 150f), OnPlay);
        MenuUI.CreateButton(mainPanel.transform, "Settings", new Vector2(0f, 0f), () => ShowSettings(true));
        MenuUI.CreateButton(mainPanel.transform, "Quit", new Vector2(0f, -150f), OnQuit);

        settings = SettingsPanel.Create(canvas.transform, () => ShowSettings(false));
        ShowSettings(false);
    }

    private void ShowSettings(bool show)
    {
        if (settings != null)
        {
            settings.Show(show);
        }

        if (mainPanel != null)
        {
            mainPanel.SetActive(!show);
        }
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
