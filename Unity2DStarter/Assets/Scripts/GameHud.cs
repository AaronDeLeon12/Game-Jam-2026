using UnityEngine;

public class GameHud : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    private PlayerCombat playerCombat;
    private Texture2D healthUiTexture;
    private float lastHealth = -1f;
    private float flashHealth;
    private float healthFlashTimer;
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
        UpdateHealthFlash();

        DrawResourceBar(new Rect(92f, 38f, 350f, 48f), playerStats.Health / playerStats.MaxHealth, Color.red, hudAlpha, 0);
        DrawHealthLossFlash(new Rect(92f, 38f, 350f, 48f), hudAlpha);
        DrawResourceBar(new Rect(92f, 112f, 350f, 48f), playerStats.Mana / playerStats.MaxMana, Color.blue, hudAlpha, 1);
        DrawEquippedSpell(new Rect(472f, 56f, 72f, 72f), hudAlpha);

        if (playerStats.IsDead)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 36,
                font = MenuUI.GameFont,
                normal = { textColor = Color.white }
            };

            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "GAME OVER", style);
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

    private void DrawResourceBar(Rect rect, float percent, Color fillColor, float alpha, int iconIndex)
    {
        EnsureHealthUiTexture();
        if (healthUiTexture != null)
        {
            Rect iconRect = new Rect(rect.x - 58f, rect.y - 4f, 52f, 52f);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTextureWithTexCoords(iconRect, healthUiTexture, GetHealthIconTexCoords(iconIndex));
        }

        DrawBar(rect, percent, fillColor, alpha);
    }

    private void DrawHealthLossFlash(Rect rect, float alpha)
    {
        if (healthFlashTimer <= 0f || playerStats == null || playerStats.MaxHealth <= 0f)
        {
            return;
        }

        float currentPercent = Mathf.Clamp01(playerStats.Health / playerStats.MaxHealth);
        float flashPercent = Mathf.Clamp01(flashHealth / playerStats.MaxHealth);
        if (flashPercent <= currentPercent)
        {
            return;
        }

        float fade = Mathf.Clamp01(healthFlashTimer / HealthFlashDuration);
        float x = rect.x + 2f + (rect.width - 4f) * currentPercent;
        float width = (rect.width - 4f) * (flashPercent - currentPercent);
        GUI.color = new Color(1f, 1f, 1f, alpha * fade);
        GUI.DrawTexture(new Rect(x, rect.y + 2f, width, rect.height - 4f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void UpdateHealthFlash()
    {
        if (playerStats == null)
        {
            return;
        }

        if (lastHealth < 0f)
        {
            lastHealth = playerStats.Health;
            flashHealth = playerStats.Health;
            return;
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
    }

    private void EnsureHealthUiTexture()
    {
        if (healthUiTexture == null)
        {
            healthUiTexture = Resources.Load<Texture2D>("UI/HealthBarUI");
        }
    }

    private static Rect GetHealthIconTexCoords(int iconIndex)
    {
        const float iconCount = 3f;
        float width = 1f / iconCount;
        int clamped = Mathf.Clamp(iconIndex, 0, 2);
        return new Rect(width * clamped, 0f, width, 1f);
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
