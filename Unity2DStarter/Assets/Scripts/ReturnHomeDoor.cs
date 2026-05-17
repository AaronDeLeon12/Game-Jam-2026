using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Assign this to the scenario's "Wall Left" so the left edge of the level
/// works like the home door: pressing E near it shows a confirmation, and
/// "Yes" advances the day (DayManager) and loads the home scene — so the
/// player returns home on the NEXT day.
///
/// Same interaction pattern and confirm UI as DoorInteractable; the wall
/// keeps whatever collider it already has (it can stay a solid barrier).
/// PlayerInteract finds it by proximity, so no trigger collider is needed.
/// </summary>
public class ReturnHomeDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private string homeScene = "home_day_1";
    [SerializeField] private string promptText = "Press E to go back home";
    [SerializeField] private string confirmQuestion = "You will go back home and advance a day";
    [Tooltip("Also pop the prompt automatically when the player reaches/touches the wall (collision or trigger), not only on E.")]
    [SerializeField] private bool promptOnContact = true;

    private bool awaitingConfirm;

    public void Configure(string newHomeScene, string newPromptText, string newConfirmQuestion)
    {
        homeScene = newHomeScene;
        promptText = newPromptText;
        confirmQuestion = newConfirmQuestion;
    }

    public void Interact()
    {
        awaitingConfirm = true;
        GameModal.Open();
    }

    public string GetPromptText() => promptText;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryPromptFrom(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryPromptFrom(other);
    }

    private void TryPromptFrom(Collider2D other)
    {
        if (!promptOnContact || awaitingConfirm || GameModal.IsOpen || other == null)
        {
            return;
        }

        // Only react to the player.
        if (other.GetComponentInParent<PlayerStats>() != null
            || other.attachedRigidbody != null
               && other.attachedRigidbody.GetComponent<PlayerStats>() != null)
        {
            awaitingConfirm = true;
            GameModal.Open();
        }
    }

    private void GoHomeNextDay()
    {
        awaitingConfirm = false;
        GameModal.Close();

        // Advance the day first so the home scene rebuilds for the new day.
        DayManager.EnsureExists();
        if (DayManager.Instance != null)
        {
            DayManager.Instance.AdvanceDay();
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(homeScene);
        }
        else
        {
            SceneManager.LoadScene(homeScene);
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
            GoHomeNextDay();
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
