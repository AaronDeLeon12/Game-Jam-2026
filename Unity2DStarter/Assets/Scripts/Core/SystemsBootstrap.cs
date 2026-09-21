using UnityEngine;
[DefaultExecutionOrder(-1000)]
public class SystemsBootstrap : MonoBehaviour
{
    public static SystemsBootstrap Instance { get; private set; }
    [SerializeField] private GameObject player;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GameHudCanvas hud;
    public GameObject Player => player;
    public PlayerStats PlayerStats => player != null ? player.GetComponent<PlayerStats>() : null;
    public Camera GameCamera => gameCamera;
    public GameObject Hud => hud != null ? hud.gameObject : null;
    public static void EnsureExists()
    {
        if (Instance != null) return;
        var authored = FindAnyObjectByType<SystemsBootstrap>();
        if (authored != null) { authored.Initialize(); return; }
        var prefab = Resources.Load<GameObject>("Prefabs/GameplayRig");
        if (prefab == null) { Debug.LogError("Missing GameplayRig prefab. Campaign scenes must contain a GameplayRig instance."); return; }
        Instantiate(prefab);
    }
    private void Awake() { Initialize(); }
    private void Initialize()
    {
        if (Instance == this) return;
        if (Instance != null) { gameObject.SetActive(false); Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        AudioManager.EnsureExists();
        if (player == null || gameCamera == null || hud == null) { Debug.LogError("GameplayRig has missing serialized references.", this); return; }
        gameCamera.GetComponent<CameraFollow2D>().SetTarget(player.transform);
        hud.SetPlayerStats(PlayerStats);
    }
    public static GameObject PrepareForMainMenuReturn()
    {
        if (Instance == null) return null;
        Instance.GetComponent<LevelManager>().PrepareForTeardown();
        Instance.gameObject.SetActive(false);
        return Instance.gameObject;
    }
    public static void Teardown()
    {
        if (Instance == null) return;
        var root = PrepareForMainMenuReturn();
        Instance = null;
        Destroy(root);
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }
}
