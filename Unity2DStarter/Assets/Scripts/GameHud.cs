using UnityEngine;

public class GameHud : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

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
}
