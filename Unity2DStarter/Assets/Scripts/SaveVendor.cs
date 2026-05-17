using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SaveVendor : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "Press E to save";

    private bool menuOpen;
    private bool dialogueOpen;
    private bool ownsModal;
    private string dialogueLine;
    private float dialogueInputReadyTime;
    private int pendingSlot;
    private float busyOverlayUntil;
    private readonly string[] greetings =
    {
        "You can always start over... I think",
        "Bet you could have used this before...",
        "Hey you...",
        "Again?",
        "..."
    };

    public string GetPromptText()
    {
        return promptText;
    }

    public void Interact()
    {
        SessionStats.Record("save_vendor_interactions");
        dialogueLine = greetings[Random.Range(0, greetings.Length)];
        dialogueOpen = true;
        ownsModal = true;
        dialogueInputReadyTime = Time.realtimeSinceStartup + 0.15f;
        GameModal.Open();
        pendingSlot = 0;
    }

    private void Awake()
    {
        Collider2D bodyCollider = GetComponent<Collider2D>();
        bodyCollider.isTrigger = true;
    }

    private void OnGUI()
    {
        if (!menuOpen && !dialogueOpen && Time.realtimeSinceStartup > busyOverlayUntil)
        {
            if (ownsModal && GameModal.IsOpen)
            {
                GameModal.Close();
            }

            ownsModal = false;

            return;
        }

        if (Time.realtimeSinceStartup <= busyOverlayUntil)
        {
            DrawBusyOverlay();
            return;
        }

        if (dialogueOpen)
        {
            DrawVendorDialogue();
            return;
        }

        DrawSaveMenu();
    }

    private void DrawVendorDialogue()
    {
        GUI.color = new Color(0f, 0f, 0f, 0.35f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        Rect box = new Rect(Screen.width * 0.12f, Screen.height - 190f, Screen.width * 0.76f, 145f);
        GUI.color = new Color(0.03f, 0.03f, 0.04f, 0.92f);
        GUI.DrawTexture(box, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(box.x + 24f, box.y + 18f, box.width - 48f, 70f), dialogueLine, MenuUI.MakeLabelStyle(36));
        GUI.Label(new Rect(box.x + 24f, box.y + 88f, box.width - 48f, 40f), "Press E", MenuUI.MakeLabelStyle(24));

        Event e = Event.current;
        if (Time.realtimeSinceStartup < dialogueInputReadyTime)
        {
            return;
        }

        if (e != null
            && e.type == EventType.KeyDown
            && (e.keyCode == KeyCode.E || e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Space))
        {
            OpenSaveMenuAfterDialogue();
            e.Use();
        }
        else if (e != null && e.type == EventType.MouseDown)
        {
            OpenSaveMenuAfterDialogue();
            e.Use();
        }
    }

    private void OpenSaveMenuAfterDialogue()
    {
        dialogueOpen = false;
        menuOpen = true;
        pendingSlot = 0;
    }

    private void DrawSaveMenu()
    {

        GUI.color = new Color(0f, 0f, 0f, 0.85f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // --- TITLE ---
        GUI.Label(MenuUI.CenteredRect(100f, 900f, 150f), "SAVE DATA", MenuUI.MakeLabelStyle(60));

        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(35);
        Color originalBgColor = GUI.backgroundColor;

        if (pendingSlot == 0)
        {
            // --- DRAW SLOTS ---
            for (int slot = 1; slot <= SaveSystem.SlotCount; slot++)
            {
                // Starts at Y=280 and moves down by 110 pixels per slot
                float yPos = 280f + (slot - 1) * 110f; 
                
                // Rich Forest Green
                GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
                if (GUI.Button(MenuUI.CenteredRect(yPos, 920f, 75f), SaveSystem.GetSlotSummary(slot), buttonStyle))
                {
                    pendingSlot = slot;
                }
            }

            // --- CANCEL BUTTON ---
            // Places it dynamically underneath all the slots
            float cancelY = 280f + (SaveSystem.SlotCount) * 110f + 40f; 
            
            // Soft Brick/Berry Red
            GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
            if (GUI.Button(MenuUI.CenteredRect(cancelY, 620f, 75f), "Cancel", buttonStyle))
            {
                CloseMenu();
            }
        }
        else
        {
            // --- CONFIRMATION SCREEN ---
            
            // Height is set to 150f here as well to prevent font chopping
            GUI.Label(
                MenuUI.CenteredRect(350f, 900f, 150f), 
                "Overwrite Slot " + pendingSlot + "?", 
                MenuUI.MakeLabelStyle(48)
            );

            // Math to perfectly center the two Yes/No buttons side-by-side
            float btnWidth = 290f;
            float spacing = 40f;
            float totalWidth = (btnWidth * 2) + spacing;
            float startX = (Screen.width - totalWidth) / 2f;
            float btnY = 520f;

            // --- Yes Button : Rich Forest Green ---
            GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
            if (GUI.Button(new Rect(startX, btnY, btnWidth, 75f), "Yes", buttonStyle))
            {
                SaveToSlot(pendingSlot);
            }

            // --- No Button : Soft Brick/Berry Red ---
            GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
            if (GUI.Button(new Rect(startX + btnWidth + spacing, btnY, btnWidth, 75f), "No", buttonStyle))
            {
                pendingSlot = 0;
            }
        }

        // Restore default color
        GUI.backgroundColor = originalBgColor;
    }

    private void SaveToSlot(int slot)
    {
        PlayerStats stats = SystemsBootstrap.Instance != null && SystemsBootstrap.Instance.PlayerStats != null
            ? SystemsBootstrap.Instance.PlayerStats
            : FindAnyObjectByType<PlayerStats>();
        if (stats != null)
        {
            stats.RestoreFullHealth();
        }

        SessionStats.Record("saves_made");
        SaveSystem.WriteSlot(slot);
        menuOpen = false;
        dialogueOpen = false;
        pendingSlot = 0;
        busyOverlayUntil = Time.realtimeSinceStartup + 1.15f;
    }

    private void CloseMenu()
    {
        menuOpen = false;
        dialogueOpen = false;
        if (ownsModal)
        {
            ownsModal = false;
            GameModal.Close();
        }
    }

    private void DrawBusyOverlay()
    {
        GUI.color = new Color(0f, 0f, 0f, 0.82f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        
        // Switched to MenuUI to keep the font looking identical
        GUI.Label(
            new Rect(0f, 0f, Screen.width, Screen.height),
            "Saving...\nDo not turn off or quit while saving.",
            MenuUI.MakeLabelStyle(42)
        );
    }
}
