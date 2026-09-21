using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// All panels, labels and controls are saved in the Canvas prefab; this component only changes state.
public class AuthoredMenuCanvas : MonoBehaviour
{
    public static AuthoredMenuCanvas Instance { get; private set; }
    public bool mainMenu;
    public GameObject rootPanel, playPanel, difficultyPanel, settingsPanel, slotsPanel, confirmationPanel, dialoguePanel;
    public Text confirmationText, dialogueText, volumeText, fullscreenText, promptText, dayText;
    public Button[] slotButtons, deleteButtons;
    private GameObject previousPanel;
    private Action confirmed, dialogueNext;
    private bool saving;
    private bool wasPaused;
    private float dialogueReady;
    private void Awake()
    {
        Instance = this;
        GameSettings.Load();
        HidePanels();
        if (mainMenu) { rootPanel.SetActive(true); GameAudio.PlayMusic("MainMenu", .45f); }
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }
    private void Update()
    {
        if (!mainMenu)
        {
            bool paused = PauseMenu.IsPaused;
            if (paused != wasPaused) { HidePanels(); rootPanel.SetActive(paused); wasPaused = paused; }
            var systems = SystemsBootstrap.Instance;
            var interact = systems != null && systems.Player != null ? systems.Player.GetComponent<PlayerInteract>() : null;
            if (promptText != null) promptText.text = !GameModal.IsOpen && !paused && interact != null ? interact.GetCurrentPrompt() : "";
            if (dayText != null) dayText.text = HomeMode.IsActive && DayManager.Instance != null ? "Day " + DayManager.Instance.CurrentDay : "";
        }
        if (!npcOwnsInput && dialoguePanel.activeSelf && Time.unscaledTime >= dialogueReady && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))) NextDialogue();
    }
    private void HidePanels()
    {
        foreach (var panel in new[] { rootPanel, playPanel, difficultyPanel, settingsPanel, slotsPanel, confirmationPanel, dialoguePanel }) if (panel != null) panel.SetActive(false);
    }
    public void ShowRoot() { HidePanels(); rootPanel.SetActive(true); }
    public void ShowPlay() { HidePanels(); playPanel.SetActive(true); }
    public void ShowDifficulty() { HidePanels(); difficultyPanel.SetActive(true); }
    public void ShowSettings() { HidePanels(); settingsPanel.SetActive(true); RefreshSettings(); }
    public void VolumeUp() { GameSettings.ChangeVolume(.1f); RefreshSettings(); }
    public void VolumeDown() { GameSettings.ChangeVolume(-.1f); RefreshSettings(); }
    public void Fullscreen() { GameSettings.ToggleFullscreen(); RefreshSettings(); }
    public void Windowed() { GameSettings.SetWindowed(); RefreshSettings(); }
    private void RefreshSettings() { volumeText.text = "Volume: " + Mathf.RoundToInt(GameSettings.Volume * 100) + "%"; fullscreenText.text = "Fullscreen: " + (GameSettings.Fullscreen ? "On" : "Off"); }
    public void NewEasy() { NewGame(GameDifficulty.Easy); }
    public void NewNormal() { NewGame(GameDifficulty.Normal); }
    public void NewHard() { NewGame(GameDifficulty.Hard); }
    private void NewGame(GameDifficulty difficulty)
    {
        DayManager.EnsureExists(); DayManager.Instance.StartNewGame(); GameSession.StartNew(difficulty);
        GameModal.Close(); Time.timeScale = 1; AudioListener.pause = false;
        SceneManager.LoadScene("home_day_1");
    }
    public void Wrath() { GameSession.StartNew(GameDifficulty.Normal); Time.timeScale = 1; SceneManager.LoadScene("SurvivalTesting"); }
    public void Resume() { var pause = SystemsBootstrap.Instance.GetComponent<PauseMenu>(); pause.ResumeFromCanvas(); HidePanels(); }
    public void ReturnToMenu() { Confirm("Return to Main Menu? Unsaved progress will be lost.", () => { Resume(); SystemsBootstrap.Teardown(); SceneManager.LoadScene("MainMenu"); }); }
    public void Quit() { Confirm("Quit to desktop? Unsaved progress will be lost.", () => {
        Time.timeScale = 1; AudioListener.pause = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }); }
    public void Confirm(string message, Action action)
    {
        previousPanel = rootPanel.activeSelf ? rootPanel : slotsPanel.activeSelf ? slotsPanel : null;
        confirmed = action; confirmationText.text = message;
        if (previousPanel != null) previousPanel.SetActive(false);
        confirmationPanel.SetActive(true); GameModal.Open();
    }
    public void Accept() { var action = confirmed; CancelConfirmation(); action?.Invoke(); }
    public void CancelConfirmation() { confirmationPanel.SetActive(false); confirmed = null; if (previousPanel != null) previousPanel.SetActive(true); GameModal.Close(); if (saving && slotsPanel.activeSelf) GameModal.Open(); }
    public void ShowLoad() { saving = false; ShowSlots(); }
    public void ShowSave() { saving = true; GameModal.Open(); ShowSlots(); }
    private void ShowSlots()
    {
        HidePanels(); slotsPanel.SetActive(true);
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slot = i + 1;
            slotButtons[i].GetComponentInChildren<Text>().text = SaveSystem.GetSlotSummary(slot);
            slotButtons[i].interactable = saving || SaveSystem.HasSave(slot);
            deleteButtons[i].gameObject.SetActive(!saving);
            deleteButtons[i].interactable = SaveSystem.HasSave(slot);
        }
    }
    public void SelectSlot(int slot)
    {
        Confirm((saving ? "Save over slot " : "Load slot ") + slot + "?", () => {
            if (saving) { SystemsBootstrap.Instance.PlayerStats.RestoreFullHealth(); SessionStats.Record("saves_made"); SaveSystem.WriteSlot(slot); CloseSlots(); }
            else { var save = SaveSystem.ReadSlot(slot); if (save != null) { Time.timeScale = 1; AudioListener.pause = false; GameSession.LoadFromSave(save); } }
        });
    }
    public void DeleteSlot(int slot) { Confirm("Permanently delete slot " + slot + "?", () => { SaveSystem.DeleteSlot(slot); ShowSlots(); }); }
    public void CloseSlots() { HidePanels(); GameModal.Close(); if (mainMenu) ShowPlay(); }
    public void ShowDialogue(string text, Action next)
    {
        npcOwnsInput = false; dialogueText.text = text; dialogueNext = next; dialoguePanel.SetActive(true); dialogueReady = Time.unscaledTime + .15f;
    }
    public void ShowNpcDialogue(string text, Action next)
    {
        if (!dialoguePanel.activeSelf || dialogueText.text != text) ShowDialogue(text, next);
        npcOwnsInput = true;
    }
    private bool npcOwnsInput;
    public void NextDialogue() { if (Time.unscaledTime < dialogueReady) return; var next = dialogueNext; dialoguePanel.SetActive(false); dialogueNext = null; next?.Invoke(); }
    public void HideDialogue() { dialoguePanel.SetActive(false); dialogueNext = null; }
}
