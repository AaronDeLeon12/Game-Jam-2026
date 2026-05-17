using UnityEngine;
using UnityEngine.Rendering.Universal;

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
        AudioManager.EnsureExists();
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

        PrepareForMainMenuReturn();

        if (Instance != null)
        {
            GameObject systemsObject = Instance.gameObject;
            Instance = null;
            Destroy(systemsObject);
        }
    }

    public static GameObject PrepareForMainMenuReturn()
    {
        if (Instance == null)
        {
            return null;
        }

        LevelManager levelManager = Instance.GetComponent<LevelManager>();
        if (levelManager != null)
        {
            levelManager.PrepareForTeardown();
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

        return Instance.gameObject;
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
        BuildGlobalLight();
    }

    private void BuildGlobalLight()
    {
        // If the scene already has a global light (e.g. one placed by hand in
        // the editor so the scene is visible while building), use that and do
        // not stack a second one.
        foreach (Light2D existing in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
        {
            if (existing.lightType == Light2D.LightType.Global)
            {
                return;
            }
        }

        GameObject lightObj = new GameObject("Global Light 2D");
        lightObj.transform.SetParent(transform, false);

        Light2D light = lightObj.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        // Dim, slightly cool ambient: the world is dark by default so that
        // local lights (e.g. inside the house) read as bright/warm.
        light.intensity = 0.45f;
        light.color = new Color(0.62f, 0.67f, 0.85f);
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

        PlayerSpriteAnimator spriteAnimator = GetOrAdd<PlayerSpriteAnimator>(Player);
        spriteAnimator.ForceRefresh();
        PlayerStats = GetOrAdd<PlayerStats>(Player);
        GetOrAdd<PlayerActionCounter>(Player);

        if (Player.GetComponent<PlayerMovement2D>() == null)
        {
            Player.AddComponent<PlayerMovement2D>();
        }

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

        SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = visual.gameObject.AddComponent<SpriteRenderer>();
        }

        Sprite idleSprite = PlayerSpriteAnimator.LoadWalkSprite(1);
        if (idleSprite != null)
        {
            renderer.sprite = idleSprite;
            renderer.color = Color.white;
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.sortingOrder = 10;

            float spriteHeight = idleSprite.bounds.size.y;
            if (spriteHeight > 0f)
            {
                float scale = 1.45f / spriteHeight;
                visual.localScale = new Vector3(scale, scale, 1f);
            }
        }
        else
        {
            PlaceholderSprites.MakeSquare(visual.gameObject, new Color(0.95f, 0.95f, 0.95f), 10);
        }
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
