using UnityEngine;

public class CirclePopup : MonoBehaviour
{
    [SerializeField] private float radius = 3f;
    [SerializeField] private Color color = new Color(1f, 0.8f, 0.2f, 0.6f);
    [SerializeField] private int sortingOrder = 50;

    private GameObject visual;
    private bool isVisible;

    private void Awake()
    {
        visual = new GameObject("CirclePopup_Visual");
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * radius * 2f;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;

        visual.SetActive(false);
    }

    public void Toggle()
    {
        isVisible = !isVisible;
        visual.SetActive(isVisible);
    }

    private Sprite CreateCircleSprite()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radiusSq = (center - 0.5f) * (center - 0.5f);

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                pixels[y * size + x] = (dx * dx + dy * dy <= radiusSq) ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
