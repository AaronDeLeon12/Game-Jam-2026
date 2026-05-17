using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A door the player interacts with (press E when near). Interacting shows a
/// confirmation prompt; only "Yes" loads the target scene.
/// </summary>
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string targetScene = "outside_1";
    [SerializeField] private string promptText = "Press E to go outside";
    [SerializeField] private string confirmQuestion = "Are you sure you want to go outside?";

    private bool awaitingConfirm;

    public void Interact()
    {
        awaitingConfirm = true;
    }

    public string GetPromptText() => promptText;

    private void GoOutside()
    {
        awaitingConfirm = false;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(targetScene);
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }
    }

    private void OnGUI()
    {
        if (!awaitingConfirm)
        {
            return;
        }

        const float w = 480f;
        const float h = 190f;
        Rect rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);

        GUI.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(rect.x + 24f, rect.y + 26f, rect.width - 48f, 74f), confirmQuestion, labelStyle);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };
        float bw = 170f;
        float bh = 52f;
        float by = rect.y + rect.height - bh - 24f;

        if (GUI.Button(new Rect(rect.center.x - bw - 12f, by, bw, bh), "Yes", buttonStyle))
        {
            GoOutside();
        }

        if (GUI.Button(new Rect(rect.center.x + 12f, by, bw, bh), "No", buttonStyle))
        {
            awaitingConfirm = false;
        }
    }
}
