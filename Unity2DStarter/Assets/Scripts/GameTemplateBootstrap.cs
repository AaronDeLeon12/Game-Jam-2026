using UnityEngine;

public class GameTemplateBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            player = new GameObject("Player");
        }

        SetupPlayer(player);
        SetupFloor();
        SetupDummy();
        SetupMovementTestArea();
        SetupShooter();
        SetupCamera(player.transform);
        SetupHud(player.GetComponent<PlayerStats>());
        RemoveOldPlatforms();
    }

    private static void SetupPlayer(GameObject player)
    {
        player.transform.position = new Vector3(0f, -1.75f, 0f);
        player.transform.localScale = Vector3.one;
        SetupPlayerVisual(player);

        Rigidbody2D body = GetOrAdd<Rigidbody2D>(player);
        body.gravityScale = 5f;
        body.freezeRotation = true;

        BoxCollider2D collider = GetOrAdd<BoxCollider2D>(player);
        collider.isTrigger = false;
        collider.size = Vector2.one;

        if (player.GetComponent<PlayerMovement2D>() == null)
        {
            player.AddComponent<PlayerMovement2D>();
        }

        if (player.GetComponent<PlayerStats>() == null)
        {
            player.AddComponent<PlayerStats>();
        }

        if (player.GetComponent<PlayerCombat>() == null)
        {
            player.AddComponent<PlayerCombat>();
        }
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

    private static void SetupFloor()
    {
        GameObject floor = GameObject.Find("Infinite Floor");
        if (floor == null)
        {
            floor = GameObject.Find("Ground Platform");
        }

        if (floor == null)
        {
            floor = new GameObject("Infinite Floor");
        }

        floor.name = "Infinite Floor";
        floor.transform.position = new Vector3(0f, -3f, 0f);
        floor.transform.localScale = new Vector3(2000f, 1f, 1f);
        PlaceholderSprites.MakeSquare(floor, new Color(0.28f, 0.28f, 0.32f), 0);

        BoxCollider2D collider = GetOrAdd<BoxCollider2D>(floor);
        collider.isTrigger = false;
        collider.size = Vector2.one;
        GetOrAdd<GroundSurface2D>(floor);
    }

    private static void SetupDummy()
    {
        GameObject dummy = GameObject.Find("Enemy Dummy");
        if (dummy == null)
        {
            dummy = GameObject.Find("Beta Target Dummy");
        }

        if (dummy == null)
        {
            dummy = new GameObject("Beta Target Dummy");
        }

        dummy.name = "Beta Target Dummy";
        dummy.transform.position = new Vector3(6f, -1.75f, 0f);
        dummy.transform.localScale = Vector3.one;
        PlaceholderSprites.MakeSquare(dummy, new Color(0.2f, 0.9f, 0.25f), 10);

        BoxCollider2D collider = GetOrAdd<BoxCollider2D>(dummy);
        collider.isTrigger = true;
        collider.size = Vector2.one;

        if (dummy.GetComponent<EnemyDummy>() == null)
        {
            dummy.AddComponent<EnemyDummy>();
        }
    }

    private static void SetupMovementTestArea()
    {
        CreateSolidBlock("Beta Left Step Platform", new Vector3(-6f, -2.15f, 0f), new Vector3(3f, 0.35f, 1f), new Color(0.42f, 0.45f, 0.5f), true);
        CreateSolidBlock("Beta Left Tall Obstacle", new Vector3(-9f, -2f, 0f), new Vector3(0.75f, 2f, 1f), new Color(0.5f, 0.42f, 0.35f), false);
        CreateSolidBlock("Beta Duck Tunnel Ceiling", new Vector3(-13f, -1.55f, 0f), new Vector3(4.5f, 0.4f, 1f), new Color(0.35f, 0.5f, 0.48f), false);
        CreateSolidBlock("Beta Raised Left Platform", new Vector3(-17f, -0.85f, 0f), new Vector3(4f, 0.35f, 1f), new Color(0.42f, 0.45f, 0.5f), true);
        CreateSolidBlock("Beta Dash Test Wall", new Vector3(-21f, -1.85f, 0f), new Vector3(0.5f, 2.3f, 1f), new Color(0.5f, 0.42f, 0.35f), false);
    }

    private static void SetupShooter()
    {
        GameObject shooter = GameObject.Find("Beta Shooter Dummy");
        if (shooter == null)
        {
            shooter = new GameObject("Beta Shooter Dummy");
        }

        shooter.transform.position = new Vector3(12f, -1.75f, 0f);
        shooter.transform.localScale = Vector3.one;
        PlaceholderSprites.MakeSquare(shooter, new Color(0.1f, 0.7f, 0.2f), 10);

        BoxCollider2D collider = GetOrAdd<BoxCollider2D>(shooter);
        collider.isTrigger = true;
        collider.size = Vector2.one;

        if (shooter.GetComponent<EnemyDummy>() == null)
        {
            shooter.AddComponent<EnemyDummy>();
        }

        if (shooter.GetComponent<EnemyShooter>() == null)
        {
            shooter.AddComponent<EnemyShooter>();
        }
    }

    private static void CreateSolidBlock(string name, Vector3 position, Vector3 scale, Color color, bool countsAsGround)
    {
        GameObject block = GameObject.Find(name);
        if (block == null)
        {
            block = new GameObject(name);
        }

        block.transform.position = position;
        block.transform.localScale = scale;
        PlaceholderSprites.MakeSquare(block, color, 1);

        BoxCollider2D collider = GetOrAdd<BoxCollider2D>(block);
        collider.isTrigger = false;
        collider.size = Vector2.one;

        GroundSurface2D groundSurface = block.GetComponent<GroundSurface2D>();
        if (countsAsGround && groundSurface == null)
        {
            block.AddComponent<GroundSurface2D>();
        }
        else if (!countsAsGround && groundSurface != null)
        {
            Destroy(groundSurface);
        }
    }

    private static void SetupCamera(Transform player)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.backgroundColor = new Color(0.07f, 0.08f, 0.11f);
        camera.transform.position = new Vector3(player.position.x, player.position.y + 1f, -10f);

        CameraFollow2D follow = GetOrAdd<CameraFollow2D>(camera.gameObject);
        follow.SetTarget(player);
    }

    private static void SetupHud(PlayerStats stats)
    {
        GameObject hudObject = GameObject.Find("Game HUD");
        if (hudObject == null)
        {
            hudObject = new GameObject("Game HUD");
        }

        GameHud hud = GetOrAdd<GameHud>(hudObject);
        hud.SetPlayerStats(stats);
    }

    private static void RemoveOldPlatforms()
    {
        DestroyIfFound("Small Platform Left");
        DestroyIfFound("Small Platform Right");
        DestroyIfFound("High Platform");
        DestroyIfFound("Old Small Platform Left");
        DestroyIfFound("Old Small Platform Right");
        DestroyIfFound("Old High Platform");
    }

    private static void DestroyIfFound(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            Destroy(obj);
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
