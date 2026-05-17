using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu. Builds its UI from the shared MenuUI helpers and reuses the
/// shared SettingsPanel so settings behave identically to the pause menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    private const string FirstSceneName = "home_day_1";
    private const string SurvivalSceneName = "SurvivalTesting";

    private MenuState state = MenuState.Root;
    private bool showingSettings;
    
    // Explicit boolean flag makes the modal 100% reliable
    private bool isConfirming;
    private string confirmMessage;
    private System.Action confirmAction;
    
    private float loadingOverlayUntil;

    private enum MenuState
    {
        Root,
        Play,
        Difficulty,
        Load
    }

    private void Awake()
    {
        GameSettings.Load();

        EnsureCamera();
        SetupMusic();
    }

    private void Start()
    {
        EnsureCamera();
    }

    private void OnGUI()
    {
        EnsureCamera();
        DrawBackground();

        if (showingSettings)
        {
            SettingsPanel.Draw(() => showingSettings = false);
            return;
        }

        bool isModalOpen = isConfirming || (Time.realtimeSinceStartup <= loadingOverlayUntil);
        
        // Disable underlying buttons if a window is open
        if (isModalOpen) GUI.enabled = false;

        // Title box (Bottom edge ends at Y = 450)
        GUI.Label(MenuUI.CenteredRect(100f, 1000f, 350f), "Tales of Ivory Moss", MenuUI.MakeLabelStyle(72));

        switch (state)
        {
            case MenuState.Play: DrawPlayMenu(); break;
            case MenuState.Difficulty: DrawDifficultyMenu(); break;
            case MenuState.Load: DrawLoadMenu(); break;
            default: DrawRootMenu(); break;
        }

        // Always re-enable before drawing overlays
        GUI.enabled = true;

        DrawConfirmOverlay();
        DrawLoadingOverlay();
    }

    private void DrawBackground()
    {
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = new Color(0.06f, 0.07f, 0.1f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void DrawRootMenu()
    {
        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(35);
        Color originalBgColor = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
        if (GUI.Button(MenuUI.CenteredRect(500f, 620f, 75f), "Play", buttonStyle))
        {
            Press();
            state = MenuState.Play;
        }

        GUI.backgroundColor = new Color(0.62f, 0.55f, 0.95f);
        if (GUI.Button(MenuUI.CenteredRect(620f, 620f, 75f), "Wrath Mode", buttonStyle))
        {
            Press();
            Confirm("Start Wrath Mode?\n(Survival Testing)", StartWrathMode);
        }

        GUI.backgroundColor = new Color(0.9f, 0.8f, 0.4f);
        if (GUI.Button(MenuUI.CenteredRect(740f, 620f, 75f), "Settings", buttonStyle))
        {
            Press();
            showingSettings = true;
        }

        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
        if (GUI.Button(MenuUI.CenteredRect(860f, 620f, 75f), "Quit", buttonStyle))
        {
            Press();
            Confirm("Quit the game?", OnQuit);
        }

        GUI.backgroundColor = originalBgColor;
    }

    private void DrawPlayMenu()
    {
        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(35);
        Color originalBgColor = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
        if (GUI.Button(MenuUI.CenteredRect(470f, 620f, 75f), "New Game", buttonStyle))
        {
            Press();
            state = MenuState.Difficulty;
        }

        bool hasSave = SaveSystem.HasAnySave();
        GUI.enabled = hasSave && !isConfirming && (Time.realtimeSinceStartup > loadingOverlayUntil);
        GUI.backgroundColor = hasSave ? new Color(0.9f, 0.8f, 0.4f) : new Color(0.32f, 0.32f, 0.34f);
        if (GUI.Button(MenuUI.CenteredRect(590f, 620f, 75f), "Load Game", buttonStyle))
        {
            Press();
            state = MenuState.Load;
        }

        GUI.enabled = !isConfirming && (Time.realtimeSinceStartup > loadingOverlayUntil);
        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
        if (GUI.Button(MenuUI.CenteredRect(710f, 620f, 75f), "Back", buttonStyle))
        {
            Press();
            state = MenuState.Root;
        }

        GUI.backgroundColor = originalBgColor;
    }

    private void DrawDifficultyMenu()
    {
        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(35);
        Color originalBgColor = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
        if (GUI.Button(MenuUI.CenteredRect(460f, 620f, 75f), "Easy", buttonStyle))
        {
            Press();
            Confirm("Start a new Easy game?", () => StartNewGame(GameDifficulty.Easy));
        }

        GUI.backgroundColor = new Color(0.9f, 0.8f, 0.4f);
        if (GUI.Button(MenuUI.CenteredRect(580f, 620f, 75f), "Normal", buttonStyle))
        {
            Press();
            Confirm("Start a new Normal game?", () => StartNewGame(GameDifficulty.Normal));
        }

        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
        if (GUI.Button(MenuUI.CenteredRect(700f, 620f, 75f), "Hard", buttonStyle))
        {
            Press();
            Confirm("Start a new Hard game?", () => StartNewGame(GameDifficulty.Hard));
        }

        GUI.backgroundColor = new Color(0.6f, 0.65f, 0.9f);
        if (GUI.Button(MenuUI.CenteredRect(820f, 420f, 75f), "Back", buttonStyle))
        {
            Press();
            state = MenuState.Play;
        }

        GUI.backgroundColor = originalBgColor;
    }

    private void DrawLoadMenu()
    {
        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(30);
        Color originalBgColor = GUI.backgroundColor;

        // Pushed starting position down to 380f so it clears the title bounding box completely
        float startY = 380f; 
        float spacing = 95f;

        for (int i = 1; i <= SaveSystem.SlotCount; i++)
        {
            int slot = i;
            bool hasSave = SaveSystem.HasSave(slot);
            GUI.enabled = hasSave && !isConfirming && (Time.realtimeSinceStartup > loadingOverlayUntil);
            
            GUI.backgroundColor = hasSave ? new Color(0.9f, 0.8f, 0.4f) : new Color(0.32f, 0.32f, 0.34f);

            Rect rowRect = MenuUI.CenteredRect(startY + (i * spacing), 840f, 75f);
            Rect loadRect = new Rect(rowRect.x, rowRect.y, rowRect.width - 150f, rowRect.height);
            Rect deleteRect = new Rect(rowRect.xMax - 135f, rowRect.y, 135f, rowRect.height);

            if (GUI.Button(loadRect, SaveSystem.GetSlotSummary(slot), buttonStyle))
            {
                Press();
                Confirm("Load " + SaveSystem.GetSlotSummary(slot) + "?", () => LoadGame(slot));
            }

            GUI.backgroundColor = hasSave ? new Color(0.9f, 0.5f, 0.55f) : new Color(0.32f, 0.32f, 0.34f);
            if (GUI.Button(deleteRect, "Delete", MenuUI.MakeButtonStyle(28)))
            {
                Press();
                Confirm("Delete " + SaveSystem.GetSlotSummary(slot) + "?\nThis is permanent.", () => DeleteSave(slot));
            }
        }

        GUI.enabled = !isConfirming && (Time.realtimeSinceStartup > loadingOverlayUntil);
        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
        
        float backY = startY + ((SaveSystem.SlotCount + 1) * spacing) + 20f;
        if (GUI.Button(MenuUI.CenteredRect(backY, 420f, 75f), "Back", buttonStyle))
        {
            Press();
            state = MenuState.Play;
        }

        GUI.backgroundColor = originalBgColor;
    }

    private void DrawConfirmOverlay()
    {
        if (!isConfirming) return;

        // Force enable GUI here to guarantee input works
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

    private void DrawLoadingOverlay()
    {
        if (Time.realtimeSinceStartup > loadingOverlayUntil) return;

        GUI.enabled = true;
        GUI.color = new Color(0f, 0f, 0f, 0.82f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        
        GUI.Label(
            new Rect(0f, Screen.height * 0.4f, Screen.width, 300f),
            "Loading...\nDo not turn off or quit while loading.",
            MenuUI.MakeLabelStyle(48));
    }

    private void Confirm(string message, System.Action action)
    {
        confirmMessage = message;
        confirmAction = action;
        isConfirming = true;
    }

    private void ClearConfirm()
    {
        isConfirming = false;
        confirmMessage = null;
        confirmAction = null;
    }

    private void SetupMusic()
    {
        GameAudio.PlayMusic("MainMenu", 0.45f);
    }

    private static void EnsureCamera()
    {
        Camera existing = Camera.main;
        if (existing != null && existing.isActiveAndEnabled && existing.gameObject.scene.name != "DontDestroyOnLoad")
        {
            return;
        }

        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].gameObject.scene.name != "DontDestroyOnLoad" && cameras[i].isActiveAndEnabled)
            {
                cameras[i].tag = "MainCamera";
                return;
            }
        }

        if (existing != null && existing.gameObject.scene.name == "DontDestroyOnLoad")
        {
            existing.tag = "Untagged";
            existing.enabled = false;
        }

        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.backgroundColor = new Color(0.07f, 0.08f, 0.11f);
        cam.orthographic = true;
        camObj.AddComponent<AudioListener>();
    }

    private void StartNewGame(GameDifficulty difficulty)
    {
        DayManager.EnsureExists();
        DayManager.Instance.StartNewGame();
        GameSession.StartNew(difficulty);
        LoadSceneWithWarning(FirstSceneName);
    }

    private void StartWrathMode()
    {
        GameSession.StartNew(GameDifficulty.Normal);
        LoadSceneWithWarning(SurvivalSceneName);
    }

    private void LoadGame(int slot)
    {
        SaveData save = SaveSystem.ReadSlot(slot);
        if (save != null) StartCoroutine(LoadGameRoutine(save));
    }

    private void DeleteSave(int slot) { SaveSystem.DeleteSlot(slot); }

    private void LoadSceneWithWarning(string sceneName) { StartCoroutine(LoadSceneRoutine(sceneName)); }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        loadingOverlayUntil = Time.realtimeSinceStartup + 1.5f;
        yield return null;
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator LoadGameRoutine(SaveData save)
    {
        loadingOverlayUntil = Time.realtimeSinceStartup + 1.5f;
        yield return null;
        GameSession.LoadFromSave(save);
    }

    private static void Press() { GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f); }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
