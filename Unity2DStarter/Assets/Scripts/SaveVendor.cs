using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SaveVendor : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "Press E to save";

    private bool menuOpen;
    private int pendingSlot;
    private float busyOverlayUntil;

    public string GetPromptText()
    {
        return promptText;
    }

    public void Interact()
    {
        SessionStats.Record("save_vendor_interactions");
        menuOpen = true;
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
        if (!menuOpen && Time.realtimeSinceStartup > busyOverlayUntil)
        {
            if (GameModal.IsOpen)
            {
                GameModal.Close();
            }

            return;
        }

        if (Time.realtimeSinceStartup <= busyOverlayUntil)
        {
            DrawBusyOverlay();
            return;
        }

        DrawSaveMenu();
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
        pendingSlot = 0;
        busyOverlayUntil = Time.realtimeSinceStartup + 1.15f;
    }

    private void CloseMenu()
    {
        menuOpen = false;
        GameModal.Close();
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