using UnityEngine;

public class GameHud : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    private PlayerCombat playerCombat;

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

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            font = MenuUI.GameFont,
            normal = { textColor = Color.white }
        };

        float hudAlpha = PauseMenu.IsPaused ? 0.3f : 1f;

        DrawBar(new Rect(50f, 40f, 350f, 48f), playerStats.Health / playerStats.MaxHealth, Color.red, hudAlpha);
        DrawBar(new Rect(50f, 100f, 350f, 48f), playerStats.Mana / playerStats.MaxMana, Color.blue, hudAlpha);
        DrawEquippedSpell(new Rect(430f, 48f, 72f, 72f), hudAlpha);

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
