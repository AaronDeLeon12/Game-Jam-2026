using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent pause menu. Lives on the Systems object (created by
/// SystemsBootstrap) so it exists in every gameplay level and never in the
/// main menu. Esc toggles pause; every exit path restores Time.timeScale.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    private GameObject canvasRoot;
    private GameObject mainPanel;
    private SettingsPanel settings;
    private bool built;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    private void Pause()
    {
        GameAudio.PlaySfx("pauseSFX", transform.position, 0.75f);

        // The canvas is a scene object; if the level changed it was destroyed
        // while this persistent component survived, so rebuild when needed.
        if (canvasRoot == null)
        {
            built = false;
        }

        if (!built)
        {
            Build();
        }

        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        canvasRoot.SetActive(true);
        ShowSettings(false);
    }

    private void Resume()
    {
        GameAudio.PlaySfx("pauseSFX", transform.position, 0.75f);

        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (canvasRoot != null)
        {
            canvasRoot.SetActive(false);
        }
    }

    private void GoToMainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Tear down the persistent systems (player/camera/HUD + this object)
        // so they do not bleed into the main menu / duplicate next play.
        SystemsBootstrap.Teardown();
        SceneManager.LoadScene("MainMenu");
    }

    private void QuitGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Build()
    {
        MenuUI.EnsureEventSystem();

        Canvas canvas = MenuUI.CreateCanvas("Pause Canvas", 200);
        canvasRoot = canvas.gameObject;

        MenuUI.CreateStretchPanel(canvasRoot.transform, "Dim", new Color(0f, 0f, 0f, 0.65f));

        mainPanel = MenuUI.CreateStretch(canvasRoot.transform, "Pause Panel");
        MenuUI.CreateLabel(mainPanel.transform, "PAUSED", new Vector2(0f, 320f), new Vector2(1000f, 180f), 90);
        MenuUI.CreateButton(mainPanel.transform, "Resume", new Vector2(0f, 130f), Resume);
        MenuUI.CreateButton(mainPanel.transform, "Settings", new Vector2(0f, 0f), () => ShowSettings(true));
        MenuUI.CreateButton(mainPanel.transform, "Main Menu", new Vector2(0f, -130f), GoToMainMenu);
        MenuUI.CreateButton(mainPanel.transform, "Quit", new Vector2(0f, -260f), QuitGame);

        settings = SettingsPanel.Create(canvasRoot.transform, () => ShowSettings(false));
        built = true;
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

    private void OnDestroy()
    {
        // Safety: never leave the game frozen if this gets torn down.
        if (IsPaused)
        {
            IsPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
