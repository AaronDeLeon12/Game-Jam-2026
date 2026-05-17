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

    public void Configure(string newTargetScene, string newPromptText, string newConfirmQuestion)
    {
        targetScene = newTargetScene;
        promptText = newPromptText;
        confirmQuestion = newConfirmQuestion;
    }

    public void Interact()
    {
        awaitingConfirm = true;
        GameModal.Open();
    }

    public string GetPromptText() => promptText;

    private void GoOutside()
    {
        awaitingConfirm = false;
        GameModal.Close();

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

        const float w = 620f;
        const float h = 250f;
        Rect rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);

        GUI.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle labelStyle = DialogueUI.MakeLabelStyle(34, Color.white, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(rect.x + 34f, rect.y + 34f, rect.width - 68f, 100f), confirmQuestion, labelStyle);

        GUIStyle buttonStyle = DialogueUI.MakeButtonStyle(34);
        float bw = 210f;
        float bh = 64f;
        float by = rect.y + rect.height - bh - 30f;

        Color originalBgColor = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
        if (GUI.Button(new Rect(rect.center.x - bw - 12f, by, bw, bh), "Yes", buttonStyle))
        {
            GoOutside();
        }

        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.55f);
        if (GUI.Button(new Rect(rect.center.x + 12f, by, bw, bh), "No", buttonStyle))
        {
            awaitingConfirm = false;
            GameModal.Close();
        }

        GUI.backgroundColor = originalBgColor;
    }
}
