using UnityEngine;
using UnityEngine.SceneManagement;
public class OutsideScene : MonoBehaviour
{
    [SerializeField] private string bossScene = "boss_fight_scenario";
    [SerializeField] private int bossDay = 5;
    private void Awake()
    {
        DayManager.EnsureExists();
        SystemsBootstrap.EnsureExists();
        HomeMode.IsActive = false;
        var cam = SystemsBootstrap.Instance.GameCamera;
        cam.orthographicSize = 5;
        cam.GetComponent<CameraFollow2D>().SetOffset(new Vector3(0, 2.5f, -10));
    }
    private void Start()
    {
        if (DayManager.Instance.CurrentDay >= bossDay) SceneManager.LoadScene(bossScene);
    }
}
