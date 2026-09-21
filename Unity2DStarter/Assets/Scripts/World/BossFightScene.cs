using UnityEngine;
public class BossFightScene : MonoBehaviour
{
    private void Awake()
    {
        DayManager.EnsureExists();
        SystemsBootstrap.EnsureExists();
        HomeMode.IsActive = false;
        GameAudio.PlayMusic("bossFight1", 0.5f);
        var cam = SystemsBootstrap.Instance.GameCamera;
        cam.orthographicSize = 5;
        cam.GetComponent<CameraFollow2D>().SetOffset(new Vector3(0, 2.5f, -10));
    }
}
