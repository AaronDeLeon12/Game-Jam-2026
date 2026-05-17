using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHud : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    private PlayerCombat playerCombat;
    private Texture2D healthFrameTexture;
    private Texture2D manaFrameTexture;
    private float lastHealth = -1f;
    private float flashHealth;
    private float healthFlashTimer;
    private float lastMana = -1f;
    private float flashMana;
    private float manaFlashTimer;
    private bool showingLoadSlots;
    private bool returningToMainMenu;
    private const float HealthFlashDuration = 0.75f;

    public void SetPlayerStats(PlayerStats stats)
    {
        playerStats = stats;
    }

    private void OnGUI()
    {
        if (HomeMode.IsActive)
        {
            return;
        }

        if (playerStats == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                playerStats = player.GetComponent<PlayerStats>();
                playerCombat = player.GetComponent<PlayerCombat>();
            }

            if (playerStats == null)
            {
                return;
            }
        }

        float hudAlpha = PauseMenu.IsPaused ? 0.3f : 1f;
        UpdateResourceFlashes();

        DrawFramedResourceBar(new Rect(18f, 8f, 390f, 130f), healthFrameTexture, "UI/HealthBarUI", playerStats.Health, playerStats.MaxHealth, flashHealth, healthFlashTimer, Color.red, hudAlpha);
        DrawFramedResourceBar(new Rect(18f, 152f, 390f, 130f), manaFrameTexture, "UI/ManaBarUI", playerStats.Mana, playerStats.MaxMana, flashMana, manaFlashTimer, Color.blue, hudAlpha);
        DrawEquippedSpell(new Rect(430f, 100f, 72f, 72f), hudAlpha);

        if (playerStats.IsDead)
        {
            GameModal.Open();
            DrawDeathMenu();
        }
    }


    private static void DrawBar(Rect rect, float percent, Color fillColor, float alpha)
    {
        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = new Color(fillColor.r, fillColor.g, fillColor.b, alpha);
        GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, (rect.width - 4f) * Mathf.Clamp01(percent), rect.height - 4f), Texture2D.whiteTexture);

        GUI.color = Color.white;
    }

    private void DrawFramedResourceBar(Rect frameRect, Texture2D frameTexture, string resourcePath, float value, float maxValue, float flashValue, float flashTimer, Color fillColor, float alpha)
    {
        frameTexture = EnsureFrameTexture(frameTexture, resourcePath);
        if (resourcePath.EndsWith("HealthBarUI"))
        {
            healthFrameTexture = frameTexture;
        }
        else
        {
            manaFrameTexture = frameTexture;
        }

        Rect fillRect = new Rect(
            frameRect.x + frameRect.width * 0.22f,
            frameRect.y + frameRect.height * 0.42f,
            frameRect.width * 0.64f,
            frameRect.height * 0.105f);

        float percent = maxValue > 0f ? Mathf.Clamp01(value / maxValue) : 0f;
        GUI.color = new Color(fillColor.r, fillColor.g, fillColor.b, alpha * 0.92f);
        GUI.DrawTexture(new Rect(fillRect.x, fillRect.y, fillRect.width * percent, fillRect.height), Texture2D.whiteTexture);

        DrawResourceSpendFlash(fillRect, value, maxValue, flashValue, flashTimer, alpha);

        if (frameTexture != null)
        {
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(frameRect, frameTexture, ScaleMode.StretchToFill, true);
        }

        GUI.color = Color.white;
    }

    private void DrawResourceSpendFlash(Rect fillRect, float value, float maxValue, float flashValue, float flashTimer, float alpha)
    {
        if (flashTimer <= 0f || maxValue <= 0f)
        {
            return;
        }

        float currentPercent = Mathf.Clamp01(value / maxValue);
        float flashPercent = Mathf.Clamp01(flashValue / maxValue);
        if (flashPercent <= currentPercent)
        {
            return;
        }

        float fade = Mathf.Clamp01(flashTimer / HealthFlashDuration);
        float x = fillRect.x + 2f + (fillRect.width - 4f) * currentPercent;
        float width = (fillRect.width - 4f) * (flashPercent - currentPercent);
        GUI.color = new Color(1f, 1f, 1f, alpha * fade);
        GUI.DrawTexture(new Rect(x, fillRect.y + 2f, width, fillRect.height - 4f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void UpdateResourceFlashes()
    {
        if (playerStats == null)
        {
            return;
        }

        if (lastHealth < 0f)
        {
            lastHealth = playerStats.Health;
            flashHealth = playerStats.Health;
        }

        if (playerStats.Health < lastHealth)
        {
            flashHealth = lastHealth;
            healthFlashTimer = HealthFlashDuration;
        }
        else if (playerStats.Health > lastHealth)
        {
            flashHealth = playerStats.Health;
        }

        lastHealth = playerStats.Health;
        if (healthFlashTimer > 0f)
        {
            healthFlashTimer = Mathf.Max(0f, healthFlashTimer - Time.unscaledDeltaTime);
            flashHealth = Mathf.Lerp(playerStats.Health, flashHealth, healthFlashTimer / HealthFlashDuration);
        }

        if (lastMana < 0f)
        {
            lastMana = playerStats.Mana;
            flashMana = playerStats.Mana;
        }

        if (playerStats.Mana < lastMana)
        {
            flashMana = lastMana;
            manaFlashTimer = HealthFlashDuration;
        }
        else if (playerStats.Mana > lastMana)
        {
            flashMana = playerStats.Mana;
        }

        lastMana = playerStats.Mana;
        if (manaFlashTimer > 0f)
        {
            manaFlashTimer = Mathf.Max(0f, manaFlashTimer - Time.unscaledDeltaTime);
            flashMana = Mathf.Lerp(playerStats.Mana, flashMana, manaFlashTimer / HealthFlashDuration);
        }
    }

    private static Texture2D EnsureFrameTexture(Texture2D cached, string resourcePath)
    {
        if (cached != null)
        {
            return cached;
        }

        Texture2D source = Resources.Load<Texture2D>(resourcePath);
        return source != null ? CreateTransparentFrameTexture(source) : null;
    }

    private static Texture2D CreateTransparentFrameTexture(Texture2D source)
    {
        Color[] pixels = source.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (IsUiBackground(pixels[i]))
            {
                pixels[i] = Color.clear;
            }
        }

        Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    private static bool IsUiBackground(Color color)
    {
        bool veryLight = color.r > 0.78f && color.g > 0.78f && color.b > 0.78f;
        float channelSpread = Mathf.Max(color.r, Mathf.Max(color.g, color.b)) - Mathf.Min(color.r, Mathf.Min(color.g, color.b));
        return veryLight && channelSpread < 0.18f;
    }

    private void DrawDeathMenu()
    {
        GUI.enabled = true;
        GUI.color = new Color(0f, 0f, 0f, 0.72f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(MenuUI.CenteredRect(70f, 820f, 180f), "GAME OVER", MenuUI.MakeLabelStyle(62));
        GUIStyle buttonStyle = MenuUI.MakeButtonStyle(34);
        Color originalBg = GUI.backgroundColor;

        if (showingLoadSlots)
        {
            DrawDeathLoadSlots(buttonStyle);
            GUI.backgroundColor = originalBg;
            return;
        }

        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
        GUI.enabled = SaveSystem.HasAnySave();
        if (GUI.Button(MenuUI.CenteredRect(340f, 620f, 75f), "Load Game", buttonStyle))
        {
            showingLoadSlots = true;
        }

        GUI.enabled = true;
        GUI.backgroundColor = new Color(0.9f, 0.8f, 0.4f);
        if (GUI.Button(MenuUI.CenteredRect(470f, 620f, 75f), "Main Menu", buttonStyle))
        {
            StartCoroutine(ReturnToMainMenuRoutine());
        }

        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
        if (GUI.Button(MenuUI.CenteredRect(600f, 620f, 75f), "Quit to Desktop", buttonStyle))
        {
            QuitGame();
        }

        GUI.backgroundColor = originalBg;
    }

    private void DrawDeathLoadSlots(GUIStyle buttonStyle)
    {
        Color originalBg = GUI.backgroundColor;
        for (int slot = 1; slot <= SaveSystem.SlotCount; slot++)
        {
            bool hasSave = SaveSystem.HasSave(slot);
            GUI.enabled = hasSave;
            GUI.backgroundColor = hasSave ? new Color(0.5f, 0.85f, 0.5f) : new Color(0.32f, 0.32f, 0.34f);
            int loadSlot = slot;
            if (GUI.Button(MenuUI.CenteredRect(250f + slot * 82f, 720f, 60f), SaveSystem.GetSlotSummary(slot), MenuUI.MakeButtonStyle(26)))
            {
                SaveData save = SaveSystem.ReadSlot(loadSlot);
                if (save != null)
                {
                    GameModal.Close();
                    Time.timeScale = 1f;
                    AudioListener.pause = false;
                    GameSession.LoadFromSave(save);
                }
            }
        }

        GUI.enabled = true;
        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
        if (GUI.Button(MenuUI.CenteredRect(730f, 420f, 65f), "Back", buttonStyle))
        {
            showingLoadSlots = false;
        }

        GUI.backgroundColor = originalBg;
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        if (returningToMainMenu)
        {
            yield break;
        }

        returningToMainMenu = true;
        GameModal.Close();
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        yield return null;
        SystemsBootstrap.Teardown();
    }

    private static void QuitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameModal.Close();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void DrawEquippedSpell(Rect rect, float alpha)
    {
        if (playerCombat == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                playerCombat = player.GetComponent<PlayerCombat>();
            }
        }

        if (playerCombat == null)
        {
            return;
        }

        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        Color spellColor = playerCombat.EquippedSpellColor;
        GUI.color = new Color(spellColor.r, spellColor.g, spellColor.b, alpha);
        SpellType spellType = playerCombat.EquippedSpell;

        switch (spellType)
        {
            case SpellType.Triangle:
                DrawTriangle(rect);
                break;
            case SpellType.Circle:
                DrawCircle(rect);
                break;
            case SpellType.Knife:
                DrawKnife(rect);
                break;
            default:
                GUI.DrawTexture(new Rect(rect.x + 18f, rect.y + 18f, 36f, 36f), Texture2D.whiteTexture);
                break;
        }

        DrawCooldownOverlay(rect, playerCombat.CooldownRemainingPercent, alpha);
        GUI.color = Color.white;
    }

    private static void DrawCooldownOverlay(Rect rect, float percent, float alpha)
    {
        percent = Mathf.Clamp01(percent);
        if (percent <= 0f)
        {
            return;
        }

        GUI.color = new Color(0.45f, 0.45f, 0.45f, alpha * 0.72f);
        GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height * (1f - percent), rect.width, rect.height * percent), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private static void DrawTriangle(Rect rect)
    {
        for (int row = 0; row < 36; row++)
        {
            float width = Mathf.Lerp(4f, 40f, row / 35f);
            GUI.DrawTexture(new Rect(rect.center.x - width * 0.5f, rect.y + 16f + row, width, 1f), Texture2D.whiteTexture);
        }
    }

    private static void DrawCircle(Rect rect)
    {
        Vector2 center = rect.center;

        for (int row = -18; row <= 18; row++)
        {
            float halfWidth = Mathf.Sqrt(Mathf.Max(0f, 18f * 18f - row * row));
            GUI.DrawTexture(new Rect(center.x - halfWidth, center.y + row, halfWidth * 2f, 1f), Texture2D.whiteTexture);
        }
    }

    private static void DrawKnife(Rect rect)
    {
        for (int i = 0; i < 34; i++)
        {
            GUI.DrawTexture(new Rect(rect.x + 18f + i, rect.y + 50f - i, 4f, 4f), Texture2D.whiteTexture);
        }

        GUI.DrawTexture(new Rect(rect.x + 12f, rect.y + 50f, 16f, 8f), Texture2D.whiteTexture);
    }
}
