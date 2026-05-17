using UnityEngine;

/// <summary>
/// An NPC the player can talk to. Add this to any character (e.g. the cat).
/// Press E near it to start the conversation; press E again to advance each
/// line; the last line ends it. The player is frozen while talking.
/// </summary>
public class Npc : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Cat";
    [SerializeField] [TextArea(2, 4)] private string[] lines = { "Meow.", "..." };
    [SerializeField] private string promptText = "Press E to talk";

    private int index;
    private bool talking;
    private PlayerMovement2D playerMovement;

    public void Interact()
    {
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        if (!talking)
        {
            talking = true;
            index = 0;
            SetPlayerFrozen(true);
        }
        else
        {
            index++;
            if (index >= lines.Length)
            {
                EndConversation();
            }
        }
    }

    public string GetPromptText() => promptText;

    private void EndConversation()
    {
        talking = false;
        index = 0;
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

        float w = Screen.width * 0.7f;
        float h = 160f;
        Rect rect = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 30f, w, h);

        GUI.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.85f, 0.5f) }
        };
        GUI.Label(new Rect(rect.x + 22f, rect.y + 14f, rect.width - 44f, 28f), npcName, nameStyle);

        GUIStyle textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            wordWrap = true,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(rect.x + 22f, rect.y + 48f, rect.width - 44f, rect.height - 84f), lines[index], textStyle);

        GUIStyle hintStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.LowerRight,
            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
        };
        GUI.Label(new Rect(rect.x, rect.y + rect.height - 28f, rect.width - 18f, 20f),
            index < lines.Length - 1 ? "E - continue" : "E - end", hintStyle);
    }
}
