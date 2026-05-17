using System;
using UnityEngine;

/// <summary>A single line of text shown in a speech.</summary>
[Serializable]
public class Dialogue
{
    [TextArea(2, 4)] public string text;
}

/// <summary>
/// One interaction's worth of conversation: a speech is several Dialogues
/// shown one after another (advance with E) when the player talks once.
/// </summary>
[Serializable]
public class Speech
{
    public Dialogue[] dialogues;
}

/// <summary>One day's worth of conversation for the cat's story mode.</summary>
[Serializable]
public class DaySpeech
{
    public Dialogue[] dialogues;
}

/// <summary>
/// An NPC the player talks to. Pressing E "looks for an interaction" and
/// plays the next Speech; advancing E steps through that speech's dialogues.
///
/// Story mode: if <see cref="dailySpeeches"/> is filled, the line shown
/// depends on the current day from DayManager (day 1 -> index 0, ... day 4
/// -> index 3; clamped). Otherwise it falls back to the looping
/// <see cref="speeches"/> list.
/// </summary>
public class Npc : MonoBehaviour, IInteractable
{
    public const int StoryDayCount = 4;

    [SerializeField] private string npcName = "Orpheus";
    [Tooltip("Story mode: 4 entries, one per day (0 = Day 1 ... 3 = Day 4). " +
             "If empty, the looping 'speeches' list below is used instead.")]
    [SerializeField] private DaySpeech[] dailySpeeches = new DaySpeech[StoryDayCount];
    [SerializeField] private Speech[] speeches;
    [SerializeField] private string promptText = "Press E to talk";

    private int speechIndex;
    private int dialogueIndex;
    private bool talking;
    private float inputUnlockTime;
    private PlayerMovement2D playerMovement;

    // Only treat it as story mode if at least one day actually has a line,
    // so an existing NPC that still uses 'speeches' keeps working.
    private bool UseDailySpeeches
    {
        get
        {
            if (dailySpeeches == null) return false;
            foreach (DaySpeech d in dailySpeeches)
            {
                if (d != null && d.dialogues != null && d.dialogues.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private int CurrentStoryDay()
    {
        int day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;
        return Mathf.Clamp(day, 1, StoryDayCount);
    }

    private void Update()
    {
        if (talking && Time.unscaledTime >= inputUnlockTime && Input.GetKeyDown(KeyCode.E))
        {
            AdvanceDialogue();
        }
    }

    public void Interact()
    {
        if (!UseDailySpeeches && (speeches == null || speeches.Length == 0))
        {
            return;
        }

        if (!talking)
        {
            SessionStats.Record("npc_talks");
            SessionStats.Record("npc_talks_" + npcName);
            StartSpeech();
        }
        else
        {
            AdvanceDialogue();
        }
    }

    public string GetPromptText() => promptText;

    private Dialogue[] CurrentDialogues()
    {
        if (UseDailySpeeches)
        {
            int idx = CurrentStoryDay() - 1;
            DaySpeech d = idx >= 0 && idx < dailySpeeches.Length ? dailySpeeches[idx] : null;
            return d != null && d.dialogues != null ? d.dialogues : Array.Empty<Dialogue>();
        }

        Speech s = speeches[speechIndex];
        return s != null && s.dialogues != null ? s.dialogues : Array.Empty<Dialogue>();
    }

    private void StartSpeech()
    {
        // Story mode: just play the current day's lines (no array to skip).
        if (UseDailySpeeches)
        {
            if (CurrentDialogues().Length > 0)
            {
                talking = true;
                GameModal.Open();
                inputUnlockTime = Time.unscaledTime + 0.15f;
                dialogueIndex = 0;
                SetPlayerFrozen(true);
            }
            return;
        }

        // Skip empty speeches; bail out if none have any dialogue.
        for (int tries = 0; tries < speeches.Length; tries++)
        {
            if (CurrentDialogues().Length > 0)
            {
                talking = true;
                GameModal.Open();
                inputUnlockTime = Time.unscaledTime + 0.15f;
                dialogueIndex = 0;
                SetPlayerFrozen(true);
                return;
            }

            speechIndex = (speechIndex + 1) % speeches.Length;
        }
    }

    private void EndSpeech()
    {
        talking = false;
        GameModal.Close();
        dialogueIndex = 0;
        // Story mode: the day controls which speech plays, so don't advance
        // an index here (talking again the same day repeats that day's lines).
        if (!UseDailySpeeches)
        {
            speechIndex = (speechIndex + 1) % speeches.Length; // next speech, looping
        }
        SetPlayerFrozen(false);
    }

    private void AdvanceDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex >= CurrentDialogues().Length)
        {
            EndSpeech();
        }
    }

    private void SetPlayerFrozen(bool frozen)
    {
        if (playerMovement == null)
        {
            playerMovement = FindAnyObjectByType<PlayerMovement2D>();
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = !frozen;
        }
    }

    private void OnGUI()
    {
        if (!talking)
        {
            return;
        }

        Dialogue[] dialogues = CurrentDialogues();
        if (dialogueIndex >= dialogues.Length)
        {
            return;
        }

        float w = Screen.width * 0.9f;
        float h = Mathf.Max(310f, Screen.height * 0.38f);
        Rect rect = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 36f, w, h);

        GUI.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle textStyle = DialogueUI.MakeLabelStyle(Mathf.RoundToInt(Screen.height * 0.052f), Color.white);
        GUI.Label(new Rect(rect.x + 44f, rect.y + 42f, rect.width - 88f, rect.height - 112f),
            dialogues[dialogueIndex].text, textStyle);

        GUIStyle hintStyle = DialogueUI.MakeLabelStyle(Mathf.RoundToInt(Screen.height * 0.036f), new Color(0.7f, 0.7f, 0.7f), TextAnchor.LowerRight);
        GUI.Label(new Rect(rect.x, rect.y + rect.height - 52f, rect.width - 34f, 40f),
            dialogueIndex < dialogues.Length - 1 ? "E - continue" : "E - end", hintStyle);
    }
}
