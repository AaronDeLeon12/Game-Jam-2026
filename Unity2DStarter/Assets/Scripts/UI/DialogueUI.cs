using UnityEngine;

public static class DialogueUI
{
    private static Font dialogueFont;

    public static Font DialogueFont
    {
        get
        {
            if (dialogueFont == null)
            {
                dialogueFont = Resources.Load<Font>("Fonts/CuteFont") ?? MenuUI.GameFont;
            }

            return dialogueFont;
        }
    }

    public static GUIStyle MakeLabelStyle(int fontSize, Color textColor, TextAnchor alignment = TextAnchor.UpperLeft)
    {
        return new GUIStyle(GUI.skin.label)
        {
            font = DialogueFont,
            fontSize = fontSize,
            alignment = alignment,
            wordWrap = true,
            normal = { textColor = textColor }
        };
    }

    public static GUIStyle MakeButtonStyle(int fontSize)
    {
        return new GUIStyle(GUI.skin.button)
        {
            font = DialogueFont,
            fontSize = fontSize
        };
    }
}
