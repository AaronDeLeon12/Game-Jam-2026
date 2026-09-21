using UnityEngine;
public class HomeScene : MonoBehaviour
{
    [SerializeField] private float cameraOrthographicSize = 2.4f;
    [SerializeField] private float cameraOffsetY = 0.6f;
    private void Awake()
    {
        HomeMode.IsActive = true;
        DayManager.EnsureExists();
        SystemsBootstrap.EnsureExists();
        GameAudio.PlayMusic("MainMenuOrHouse", 0.4f);
        var cam = SystemsBootstrap.Instance.GameCamera;
        cam.orthographicSize = cameraOrthographicSize;
        cam.GetComponent<CameraFollow2D>().SetOffset(new Vector3(0, cameraOffsetY, -10));
    }
    private void OnDestroy() { HomeMode.IsActive = false; }
}
