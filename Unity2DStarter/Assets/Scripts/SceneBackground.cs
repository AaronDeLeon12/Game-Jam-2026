using UnityEngine;

/// <summary>
/// Spawns a full-screen backdrop sprite behind everything and parents it to
/// the camera so it stays put while the camera follows the player. Scales the
/// sprite to always cover the orthographic view.
///
/// Use it two ways:
///  - Put this component on any object in a scene (set Background Resource).
///  - Or call SceneBackground.Show("Backgrounds/fondo_exterior") from a
///    level-content script (OutsideScene / BossFightScene already do).
/// </summary>
public class SceneBackground : MonoBehaviour
{
    [SerializeField] private string backgroundResource = "Backgrounds/fondo_exterior";
    [SerializeField] private int sortingOrder = -1000;

    private void Start()
    {
        Show(backgroundResource, sortingOrder);
    }

    public static void Show(string resourcePath, int sortingOrder = -1000)
    {
        Camera cam = SystemsBootstrap.Instance != null && SystemsBootstrap.Instance.GameCamera != null
            ? SystemsBootstrap.Instance.GameCamera
            : Camera.main;
        if (cam == null)
        {
            return;
        }

        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            Debug.LogWarning($"SceneBackground: sprite not found at Resources/{resourcePath}");
            return;
        }

        Transform existing = cam.transform.Find("SceneBackground");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        GameObject bg = new GameObject("SceneBackground");
        bg.transform.SetParent(cam.transform, false);
        bg.transform.localPosition = new Vector3(0f, 0f, 20f); // in front of nothing; sorting handles order

        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        SpriteLit.Apply(sr);

        // Cover the orthographic view (with some margin so edges never show).
        float viewH = cam.orthographicSize * 2f;
        float viewW = viewH * cam.aspect;
        float spriteW = sprite.bounds.size.x;
        float spriteH = sprite.bounds.size.y;
        if (spriteW > 0f && spriteH > 0f)
        {
            float scale = Mathf.Max(viewW / spriteW, viewH / spriteH) * 1.08f;
            bg.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
