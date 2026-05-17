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

/// <summary>
/// An NPC the player talks to. Pressing E "looks for an interaction" and
/// plays the next Speech; advancing E steps through that speech's dialogues.
/// Speeches are played in order and loop back to the first.
/// </summary>
public class Npc : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Orpheus";
    [SerializeField] private Speech[] speeches;
    [SerializeField] private string promptText = "Press E to talk";

    private int speechIndex;
    private int dialogueIndex;
    private bool talking;
    private PlayerMovement2D playerMovement;

    public void Interact()
    {
        if (speeches == null || speeches.Length == 0)
        {
            return;
        }

        if (!talking)
        {
            StartSpeech();
        }
        else
        {
            dialogueIndex++;
            if (dialogueIndex >= CurrentDialogues().Length)
            {
                EndSpeech();
            }
        }
    }

    public string GetPromptText() => promptText;

    private Dialogue[] CurrentDialogues()
    {
        Speech s = speeches[speechIndex];
        return s != null && s.dialogues != null ? s.dialogues : Array.Empty<Dialogue>();
    }

    private void StartSpeech()
    {
        // Skip empty speeches; bail out if none have any dialogue.
        for (int tries = 0; tries < speeches.Length; tries++)
        {
            if (CurrentDialogues().Length > 0)
            {
                talking = true;
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
        dialogueIndex = 0;
        speechIndex = (speechIndex + 1) % speeches.Length; // next speech, looping
        SetPlayerFrozen(false);
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

        float w = Screen.width * 0.7f;
        float h = 160f;
        Rect rect = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 30f, w, h);

        GUI.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle nameStyle = DialogueUI.MakeLabelStyle(20, new Color(1f, 0.85f, 0.5f));
        nameStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(rect.x + 22f, rect.y + 14f, rect.width - 44f, 28f), npcName, nameStyle);

        GUIStyle textStyle = DialogueUI.MakeLabelStyle(20, Color.white);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 48f, rect.width - 44f, rect.height - 84f),
            dialogues[dialogueIndex].text, textStyle);

        GUIStyle hintStyle = DialogueUI.MakeLabelStyle(14, new Color(0.7f, 0.7f, 0.7f), TextAnchor.LowerRight);
        GUI.Label(new Rect(rect.x, rect.y + rect.height - 28f, rect.width - 18f, 20f),
            dialogueIndex < dialogues.Length - 1 ? "E - continue" : "E - end", hintStyle);
    }
}
