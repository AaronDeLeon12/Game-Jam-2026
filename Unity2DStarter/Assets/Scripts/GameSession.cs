using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }
    public static GameDifficulty CurrentDifficulty => Instance != null ? Instance.difficulty : GameDifficulty.Normal;

    [SerializeField] private GameDifficulty difficulty = GameDifficulty.Normal;

    private SaveData pendingLoad;
    private bool applyingLoad;

    public GameDifficulty Difficulty => difficulty;

    public static void EnsureExists()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject sessionObject = new GameObject("Game Session");
        sessionObject.AddComponent<GameSession>();
    }

    public static void StartNew(GameDifficulty newDifficulty)
    {
        EnsureExists();
        Instance.difficulty = newDifficulty;
        SessionStats.ResetRun();
    }

    public static void LoadFromSave(SaveData save)
    {
        if (save == null)
        {
            return;
        }

        EnsureExists();
        Instance.pendingLoad = save;
        Instance.difficulty = save.difficulty;
        SessionStats.LoadFromSave(save);

        SystemsBootstrap.EnsureExists();
        string sceneName = string.IsNullOrEmpty(save.sceneName) ? "home_day_1" : save.sceneName;
        SceneManager.LoadScene(sceneName);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingLoad == null || applyingLoad)
        {
            return;
        }

        StartCoroutine(ApplyLoadedStateAfterLevelManager());
    }

    private IEnumerator ApplyLoadedStateAfterLevelManager()
    {
        applyingLoad = true;
        yield return null;
        ApplyLoadedState();
        pendingLoad = null;
        applyingLoad = false;
    }

    private void ApplyLoadedState()
    {
        SystemsBootstrap.EnsureExists();

        DayManager.EnsureExists();
        DayManager.Instance.SetDay(pendingLoad.day);

        if (SystemsBootstrap.Instance == null || SystemsBootstrap.Instance.Player == null)
        {
            return;
        }

        GameObject player = SystemsBootstrap.Instance.Player;
        player.transform.position = new Vector3(pendingLoad.playerX, pendingLoad.playerY, pendingLoad.playerZ);

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.SetResources(pendingLoad.health, pendingLoad.mana);
        }

        PlayerActionCounter actionCounter = player.GetComponent<PlayerActionCounter>();
        if (actionCounter != null)
        {
            actionCounter.LoadCounts(pendingLoad.actionCounts);
        }

        CameraFollow2D follow = SystemsBootstrap.Instance.GameCamera != null
            ? SystemsBootstrap.Instance.GameCamera.GetComponent<CameraFollow2D>()
            : null;
        if (follow != null)
        {
            follow.Snap();
        }
    }
}
