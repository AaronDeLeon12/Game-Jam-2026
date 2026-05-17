using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Shared uGUI construction helpers so the main menu and pause menu build
/// consistent UI from code (this project constructs scenes procedurally).
/// </summary>
public static class MenuUI
{
    public static Font Font => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    public static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObj.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public static GameObject CreateStretch(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    public static Image CreateStretchPanel(Transform parent, string name, Color color)
    {
        GameObject go = CreateStretch(parent, name);
        Image img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    public static Text CreateLabel(Transform parent, string text, Vector2 pos, Vector2 size, int fontSize)
    {
        GameObject go = new GameObject("Label");
        go.transform.SetParent(parent, false);

        Text t = go.AddComponent<Text>();
        t.font = Font;
        t.text = text;
        t.fontSize = fontSize;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return t;
    }

    public static Text CreateButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick, float width = 480f)
    {
        GameObject go = new GameObject((string.IsNullOrEmpty(label) ? "Button" : label) + " Button");
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.45f, 0.7f, 1f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            GameAudio.PlaySfx("UIpressSFX", Vector3.zero, 0.7f);
            onClick?.Invoke();
        });

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(width, 110f);

        Text t = CreateLabel(go.transform, label, Vector2.zero, new Vector2(width, 110f), 44);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        return t;
    }
}
