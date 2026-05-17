using UnityEngine;

public class DialogueBox : MonoBehaviour
{
    [SerializeField] [TextArea(2, 5)] private string[] lines = { "Hello! This is a dialogue line.", "Press O to advance or close." };

    private int currentLine;
    private bool isOpen;
    private PlayerMovement2D playerMovement;

    private void Awake()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement2D>();
    }

    private void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.O))
            Advance();
    }

    public void Open()
    {
        if (lines.Length == 0) return;
        currentLine = 0;
        isOpen = true;

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    private void Advance()
    {
        currentLine++;
        if (currentLine >= lines.Length)
            Close();
    }

    private void Close()
    {
        isOpen = false;

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    private void OnGUI()
    {
        if (!isOpen) return;

        float panelHeight = Screen.height * 0.25f;
        Rect panelRect = new Rect(0, Screen.height - panelHeight, Screen.width, panelHeight);

        GUI.color = new Color(0.05f, 0.05f, 0.1f, 0.93f);
        GUI.DrawTexture(panelRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.03f);
        textStyle.font = MenuUI.GameFont;
        textStyle.normal.textColor = Color.white;
        textStyle.wordWrap = true;
        textStyle.padding = new RectOffset(30, 30, 20, 20);

        Rect textRect = new Rect(panelRect.x, panelRect.y, panelRect.width, panelRect.height - 30f);
        GUI.Label(textRect, lines[currentLine], textStyle);

        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.02f);
        hintStyle.font = MenuUI.GameFont;
        hintStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        hintStyle.alignment = TextAnchor.LowerRight;
        hintStyle.padding = new RectOffset(10, 16, 0, 10);

        GUI.Label(panelRect, "O  next / close", hintStyle);
    }
}
