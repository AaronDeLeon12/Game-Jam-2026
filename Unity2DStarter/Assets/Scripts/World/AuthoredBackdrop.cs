using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class AuthoredBackdrop : MonoBehaviour
{
    public bool followCamera = true;
    public bool fillView = true;
    [Min(1)] public float overscan = 1.08f;
    private void LateUpdate()
    {
        var cam = SystemsBootstrap.Instance != null ? SystemsBootstrap.Instance.GameCamera : Camera.main;
        var sprite = GetComponent<SpriteRenderer>().sprite;
        if (cam == null || sprite == null) return;
        if (followCamera) transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 10);
        if (fillView) { float s = Mathf.Max(cam.orthographicSize * 2 / sprite.bounds.size.y, cam.orthographicSize * 2 * cam.aspect / sprite.bounds.size.x) * overscan; transform.localScale = new Vector3(s,s,1); }
    }
}
