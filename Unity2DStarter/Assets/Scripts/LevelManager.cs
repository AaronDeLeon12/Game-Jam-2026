using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent level controller. Loads scenes and, whenever a level becomes
/// active, places the persistent player at that level's PlayerSpawnPoint and
/// removes any stray Player/Camera the scene may have shipped with, so each
/// level only needs to contain its own content.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private bool processedInitialScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (!processedInitialScene)
        {
            ProcessActiveLevel();
            processedInitialScene = true;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void PrepareForTeardown()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void LoadNextLevel()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ProcessActiveLevel();
        processedInitialScene = true;
    }

    private void ProcessActiveLevel()
    {
        SystemsBootstrap systems = SystemsBootstrap.Instance;
        if (systems == null)
        {
            return;
        }

        RemoveStrayDuplicates(systems);

        PlayerSpawnPoint spawn = FindAnyObjectByType<PlayerSpawnPoint>();
        if (spawn != null && systems.Player != null)
        {
            systems.Player.transform.position = spawn.transform.position;

            Rigidbody2D body = systems.Player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        if (systems.GameCamera != null)
        {
            CameraFollow2D follow = systems.GameCamera.GetComponent<CameraFollow2D>();
            if (follow != null)
            {
                follow.Snap();
            }
        }
    }

    private static void RemoveStrayDuplicates(SystemsBootstrap systems)
    {
        GameObject[] players = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in players)
        {
            if (obj.name == "Player" && obj != systems.Player)
            {
                Destroy(obj);
            }
        }

        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in cameras)
        {
            if (systems.GameCamera != null && cam != systems.GameCamera)
            {
                Destroy(cam.gameObject);
            }
        }
    }
}
