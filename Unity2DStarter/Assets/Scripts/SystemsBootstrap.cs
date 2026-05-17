using UnityEngine;

/// <summary>
/// Owns the persistent game systems (Player, Camera, HUD) that must exist in
/// EVERY level. Survives scene loads via DontDestroyOnLoad and is a singleton,
/// so loading another level never duplicates or loses the player.
/// Any level can guarantee systems exist by calling SystemsBootstrap.EnsureExists().
/// </summary>
public class SystemsBootstrap : MonoBehaviour
{
    public static SystemsBootstrap Instance { get; private set; }

    public GameObject Player { get; private set; }
    public PlayerStats PlayerStats { get; private set; }
    public Camera GameCamera { get; private set; }
    public GameObject Hud { get; private set; }

    public static void EnsureExists()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject systems = new GameObject("Systems");
        systems.AddComponent<SystemsBootstrap>();
        systems.AddComponent<LevelManager>();
        systems.AddComponent<PauseMenu>();
    }

    /// <summary>
    /// Destroys all persistent systems (player, camera, HUD and the Systems
    /// object itself). Call before returning to the main menu so nothing
    /// bleeds into it or duplicates on the next play session.
    /// </summary>
    public static void Teardown()
    {
        if (Instance == null)
        {
            return;
        }

        if (Instance.Player != null)
        {
            Destroy(Instance.Player);
        }

        if (Instance.GameCamera != null)
        {
            Destroy(Instance.GameCamera.gameObject);
        }

        if (Instance.Hud != null)
        {
            Destroy(Instance.Hud);
        }

        GameObject systemsObject = Instance.gameObject;
        Instance = null;
        Destroy(systemsObject);
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

        BuildPlayer();
        BuildCamera();
        BuildHud();
    }

    private void BuildPlayer()
    {
        Player = GameObject.Find("Player");
        if (Player == null)
        {
            Player = new GameObject("Player");
        }

        Player.transform.localScale = Vector3.one;
        SetupPlayerVisual(Player);

        Rigidbody2D body = GetOrAdd<Rigidbody2D>(Player);
        body.gravityScale = 5f;
        body.freezeRotation = true;

        BoxCollider2D collider = GetOrAdd<BoxCollider2D>(Player);
        collider.isTrigger = false;
        collider.size = Vector2.one;

        // Zero-friction material stops the player "sticking" to walls when
        // holding a direction into them. Movement is velocity-driven so this
        // does not affect stopping on the ground.
        PhysicsMaterial2D frictionless = new PhysicsMaterial2D("PlayerFrictionless")
        {
            friction = 0f,
            bounciness = 0f
        };
        collider.sharedMaterial = frictionless;
        body.sharedMaterial = frictionless;

        if (Player.GetComponent<PlayerMovement2D>() == null)
        {
            Player.AddComponent<PlayerMovement2D>();
        }

        PlayerStats = GetOrAdd<PlayerStats>(Player);
        GetOrAdd<PlayerActionCounter>(Player);

        if (Player.GetComponent<PlayerCombat>() == null)
        {
            Player.AddComponent<PlayerCombat>();
        }

        if (Player.GetComponent<PlayerInteract>() == null)
        {
            Player.AddComponent<PlayerInteract>();
        }

        DontDestroyOnLoad(Player);
    }

    private void BuildCamera()
    {
        GameCamera = Camera.main;
        if (GameCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            GameCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        GameCamera.orthographic = true;
        GameCamera.orthographicSize = 5f;
        GameCamera.backgroundColor = new Color(0.07f, 0.08f, 0.11f);

        CameraFollow2D follow = GetOrAdd<CameraFollow2D>(GameCamera.gameObject);
        follow.SetTarget(Player.transform);

        DontDestroyOnLoad(GameCamera.gameObject);
    }

    private void BuildHud()
    {
        Hud = GameObject.Find("Game HUD");
        if (Hud == null)
        {
            Hud = new GameObject("Game HUD");
        }

        GameHud hud = GetOrAdd<GameHud>(Hud);
        hud.SetPlayerStats(PlayerStats);

        DontDestroyOnLoad(Hud);
    }

    private static void SetupPlayerVisual(GameObject player)
    {
        SpriteRenderer rootRenderer = player.GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            Destroy(rootRenderer);
        }

        Transform visual = player.transform.Find("Player Visual");
        if (visual == null)
        {
            visual = new GameObject("Player Visual").transform;
            visual.SetParent(player.transform, false);
        }

        visual.localPosition = Vector3.zero;
        visual.localScale = Vector3.one;
        PlaceholderSprites.MakeSquare(visual.gameObject, new Color(0.95f, 0.95f, 0.95f), 10);
    }

    private static T GetOrAdd<T>(GameObject owner) where T : Component
    {
        T component = owner.GetComponent<T>();
        if (component == null)
        {
            component = owner.AddComponent<T>();
        }

        return component;
    }
}
