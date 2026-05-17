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
    
    private bool isConfirming;
    private string confirmMessage;
    private System.Action confirmAction;

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
        ClearConfirm();
    }

    private void Resume()
    {
        GameAudio.PlaySfx("pauseSFX", transform.position, 0.75f);

        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        showingSettings = false;
        ClearConfirm();
    }

    private void GoToMainMenu()
    {
        if (returningToMainMenu) return;

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
        if (!IsPaused) return;

        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (showingSettings)
        {
            SettingsPanel.Draw(() => showingSettings = false);
            return;
        }

        // Cache state locally for this GUI pass
        bool modalOpen = isConfirming;
        
        // Disable input on underlying buttons while confirming
        if (modalOpen) GUI.enabled = false;

        GUI.Label(MenuUI.CenteredRect(80f, 800f, 250f), "PAUSED", MenuUI.MakeLabelStyle(60));
        
        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(35);
        Color originalBgColor = GUI.backgroundColor;

        // --- Resume Button ---
        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f); 
        if (GUI.Button(MenuUI.CenteredRect(440f, 620f, 75f), "Resume", buttonStyle))
        {
            Resume();
        }

        // --- Settings Button ---
        GUI.backgroundColor = new Color(0.9f, 0.8f, 0.4f); 
        if (GUI.Button(MenuUI.CenteredRect(590f, 620f, 75f), "Settings", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            showingSettings = true;
        }

        // --- Main Menu Button ---
        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f); 
        if (GUI.Button(MenuUI.CenteredRect(740f, 620f, 75f), "Main Menu", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            // Explicitly wrapped in lambda to guarantee execution while timeScale is 0
            Confirm("Return to Main Menu?\nUnsaved progress will be lost.", () => GoToMainMenu());
        }

        // --- Quit Button ---
        GUI.backgroundColor = new Color(0.6f, 0.65f, 0.9f); 
        if (GUI.Button(MenuUI.CenteredRect(890f, 620f, 75f), "Quit to Desktop", buttonStyle))
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            // Explicitly wrapped in lambda 
            Confirm("Quit Game to Desktop?\nUnsaved progress will be lost.", () => QuitGame());
        }

        GUI.backgroundColor = originalBgColor;

        // Force enable before drawing the overlay
        GUI.enabled = true;

        if (modalOpen)
        {
            DrawConfirmOverlay();
        }
    }

    private void Confirm(string message, System.Action action)
    {
        confirmMessage = message;
        confirmAction = action;
        isConfirming = true;
        
        // Forces IMGUI to drop focus on the button so the state updates instantly
        GUI.FocusControl(null);
    }

    private void ClearConfirm()
    {
        isConfirming = false;
        confirmMessage = null;
        confirmAction = null;
    }

    private void DrawConfirmOverlay()
    {
        // Force GUI enabled here to guarantee functionality
        GUI.enabled = true;

        Rect rect = new Rect((Screen.width - 800f) * 0.5f, (Screen.height - 400f) * 0.5f, 800f, 400f);
        
        GUI.color = new Color(0f, 0f, 0f, 0.72f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = new Color(0.05f, 0.05f, 0.1f, 0.96f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(rect.x + 40f, rect.y + 40f, rect.width - 80f, 200f), confirmMessage, MenuUI.MakeLabelStyle(40));

        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(35);
        Color originalBgColor = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
        if (GUI.Button(new Rect(rect.center.x - 240f, rect.y + 260f, 220f, 75f), "Yes", buttonStyle))
        {
            System.Action action = confirmAction;
            ClearConfirm();
            action?.Invoke();
        }

        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
        if (GUI.Button(new Rect(rect.center.x + 20f, rect.y + 260f, 220f, 75f), "No", buttonStyle))
        {
            ClearConfirm();
        }

        GUI.backgroundColor = originalBgColor;
    }

    private void OnDestroy()
    {
        if (IsPaused)
        {
            IsPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}