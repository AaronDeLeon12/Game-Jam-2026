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
            normal = { textColor = Color.white }
        };

        DrawBar(new Rect(50f, 40f, 350f, 48f), playerStats.Health / playerStats.MaxHealth, Color.red);
        DrawBar(new Rect(50f, 100f, 350f, 48f), playerStats.Mana / playerStats.MaxMana, Color.blue);
        DrawEquippedSpell(new Rect(430f, 48f, 72f, 72f));

        if (playerStats.IsDead)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 36,
                normal = { textColor = Color.white }
            };

            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "GAME OVER", style);
        }
    }


    private static void DrawBar(Rect rect, float percent, Color fillColor)
    {
        GUI.color = Color.black;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = fillColor;
        GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, (rect.width - 4f) * Mathf.Clamp01(percent), rect.height - 4f), Texture2D.whiteTexture);

        GUI.color = Color.white;
    }

    private void DrawEquippedSpell(Rect rect)
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

        GUI.color = Color.black;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = playerCombat.EquippedSpellColor;
        SpellType spellType = playerCombat.EquippedSpell;

        switch (spellType)
        {
            case SpellType.Triangle:
                DrawTriangle(rect);
                break;
            case SpellType.Circle:
                DrawCircle(rect);
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
}
