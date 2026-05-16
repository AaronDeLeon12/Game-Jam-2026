using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Vector2 barSize = new Vector2(1f, 0.14f);
    [SerializeField] private float yOffset = 0.75f;
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    [SerializeField] private Color fillColor = new Color(0.85f, 0.2f, 0.2f, 1f);
    [SerializeField] private int sortingOrder = 60;

    private Transform root;
    private Transform fillPivot;

    private void Awake()
    {
        root = new GameObject("HealthBar_Visual").transform;

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(root, false);
        SpriteRenderer bgRenderer = bg.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = PlaceholderSprites.Square;
        bgRenderer.color = backgroundColor;
        bgRenderer.sortingOrder = sortingOrder;
        bg.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);

        fillPivot = new GameObject("FillPivot").transform;
        fillPivot.SetParent(root, false);
        fillPivot.localPosition = new Vector3(-barSize.x * 0.5f, 0f, 0f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillPivot, false);
        SpriteRenderer fillRenderer = fill.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = PlaceholderSprites.Square;
        fillRenderer.color = fillColor;
        fillRenderer.sortingOrder = sortingOrder + 1;
        fill.transform.localPosition = new Vector3(barSize.x * 0.5f, 0f, 0f);
        fill.transform.localScale = new Vector3(barSize.x * 0.94f, barSize.y * 0.7f, 1f);
    }

    private void LateUpdate()
    {
        if (root != null)
            root.position = transform.position + Vector3.up * yOffset;
    }

    public void SetFraction(float fraction)
    {
        if (fillPivot == null) return;

        fraction = Mathf.Clamp01(fraction);
        Vector3 scale = fillPivot.localScale;
        scale.x = fraction;
        fillPivot.localScale = scale;
    }

    private void OnDestroy()
    {
        if (root != null)
            Destroy(root.gameObject);
    }
}
