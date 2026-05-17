using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Persistent pause menu. Lives on the Systems object (created by
/// SystemsBootstrap) so it exists in every gameplay level and never in the
/// main menu. Esc toggles pause; every exit path restores Time.timeScale.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    private bool showingSettings;
    private bool returningToMainMenu;

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

        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        showingSettings = false;
    }

    private void Resume()
    {
        GameAudio.PlaySfx("pauseSFX", transform.position, 0.75f);

        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        showingSettings = false;
    }

    private void GoToMainMenu()
    {
        if (returningToMainMenu)
        {
            return;
        }

        returningToMainMenu = true;
        StartCoroutine(GoToMainMenuRoutine());
    }

    private IEnumerator GoToMainMenuRoutine()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        yield return null;

        GameObject systemsObject = SystemsBootstrap.PrepareForMainMenuReturn();

        yield return null;

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);

        yield return null;

        if (systemsObject != null)
        {
            SystemsBootstrap.Teardown();
        }
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

private void OnGUI()
    {
        if (!IsPaused)
        {
            return;
        }

        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (showingSettings)
        {
            SettingsPanel.Draw(() => showingSettings = false);
            return;
        }

        GUI.Label(MenuUI.CenteredRect(80f, 800f, 100f), "PAUSED", MenuUI.MakeLabelStyle(60));
        
        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(35);
        Color originalBgColor = GUI.backgroundColor;

        // --- Resume Button : Rich Forest Green ---
        // Boosted brightness so the default gray texture doesn't kill it
        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f); 
        if (GUI.Button(MenuUI.CenteredRect(440f, 620f, 75f), "Resume", buttonStyle))
        {
            Resume();
        }

        // --- Settings Button : Warm Gold ---
        GUI.backgroundColor = new Color(0.9f, 0.8f, 0.4f); 
        if (GUI.Button(MenuUI.CenteredRect(590f, 620f, 75f), "Settings", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            showingSettings = true;
        }

        // --- Main Menu Button : Soft Brick/Berry Red ---
        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f); 
        if (GUI.Button(MenuUI.CenteredRect(740f, 620f, 75f), "Main Menu", buttonStyle))
        {
            GoToMainMenu();
        }

        // --- Quit Button : Slate Blue/Purple ---
        GUI.backgroundColor = new Color(0.6f, 0.65f, 0.9f); 
        if (GUI.Button(MenuUI.CenteredRect(890f, 620f, 75f), "Quit to Desktop", buttonStyle))
        {
            QuitGame();
        }

        GUI.backgroundColor = originalBgColor;
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
