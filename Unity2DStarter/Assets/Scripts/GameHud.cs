using System.Collections.Generic;
using UnityEngine;

public class GameHud : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    private PlayerCombat playerCombat;
    private Texture2D healthUiTexture;
    private float lastHealth = -1f;
    private float flashHealth;
    private float healthFlashTimer;
    private float lastMana = -1f;
    private float flashMana;
    private float manaFlashTimer;
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

        DrawFramedResourceBar(new Rect(18f, 12f, 430f, 143f), playerStats.Health, playerStats.MaxHealth, flashHealth, healthFlashTimer, Color.red, hudAlpha);
        DrawFramedResourceBar(new Rect(18f, 88f, 430f, 143f), playerStats.Mana, playerStats.MaxMana, flashMana, manaFlashTimer, Color.blue, hudAlpha);
        DrawEquippedSpell(new Rect(476f, 68f, 72f, 72f), hudAlpha);

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

    private void DrawFramedResourceBar(Rect frameRect, float value, float maxValue, float flashValue, float flashTimer, Color fillColor, float alpha)
    {
        EnsureHealthUiTexture();

        Rect fillRect = new Rect(
            frameRect.x + frameRect.width * 0.22f,
            frameRect.y + frameRect.height * 0.39f,
            frameRect.width * 0.68f,
            frameRect.height * 0.18f);

        GUI.color = new Color(0f, 0f, 0f, alpha * 0.9f);
        GUI.DrawTexture(fillRect, Texture2D.whiteTexture);

        float percent = maxValue > 0f ? Mathf.Clamp01(value / maxValue) : 0f;
        GUI.color = new Color(fillColor.r, fillColor.g, fillColor.b, alpha);
        GUI.DrawTexture(new Rect(fillRect.x + 2f, fillRect.y + 2f, (fillRect.width - 4f) * percent, fillRect.height - 4f), Texture2D.whiteTexture);

        DrawResourceSpendFlash(fillRect, value, maxValue, flashValue, flashTimer, alpha);

        if (healthUiTexture != null)
        {
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(frameRect, healthUiTexture, ScaleMode.StretchToFill, true);
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

    private void EnsureHealthUiTexture()
    {
        if (healthUiTexture == null)
        {
            Texture2D source = Resources.Load<Texture2D>("UI/HealthBarUI");
            if (source != null)
            {
                healthUiTexture = CreateTransparentFrameTexture(source);
            }
        }
    }

    private static Texture2D CreateTransparentFrameTexture(Texture2D source)
    {
        Color[] pixels = source.GetPixels();
        bool[] background = FindUiBackgroundMask(source, pixels);

        for (int i = 0; i < pixels.Length; i++)
        {
            if (background[i])
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

    private static bool[] FindUiBackgroundMask(Texture2D texture, Color[] pixels)
    {
        bool[] visited = new bool[pixels.Length];
        Queue<int> queue = new Queue<int>();

        void TryEnqueue(int x, int y)
        {
            if (x < 0 || x >= texture.width || y < 0 || y >= texture.height)
            {
                return;
            }

            int index = y * texture.width + x;
            if (visited[index] || !IsUiBackground(pixels[index]))
            {
                return;
            }

            visited[index] = true;
            queue.Enqueue(index);
        }

        for (int x = 0; x < texture.width; x++)
        {
            TryEnqueue(x, 0);
            TryEnqueue(x, texture.height - 1);
        }

        for (int y = 0; y < texture.height; y++)
        {
            TryEnqueue(0, y);
            TryEnqueue(texture.width - 1, y);
        }

        while (queue.Count > 0)
        {
            int index = queue.Dequeue();
            int x = index % texture.width;
            int y = index / texture.width;

            TryEnqueue(x + 1, y);
            TryEnqueue(x - 1, y);
            TryEnqueue(x, y + 1);
            TryEnqueue(x, y - 1);
        }

        return visited;
    }

    private static bool IsUiBackground(Color color)
    {
        bool veryLight = color.r > 0.88f && color.g > 0.88f && color.b > 0.88f;
        float channelSpread = Mathf.Max(color.r, Mathf.Max(color.g, color.b)) - Mathf.Min(color.r, Mathf.Min(color.g, color.b));
        return veryLight && channelSpread < 0.08f;
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
