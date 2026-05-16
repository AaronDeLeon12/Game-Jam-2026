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
        SetupCamera(player.transform);
        SetupHud(player.GetComponent<PlayerStats>());
        RemoveOldPlatforms();
    }

    private static void SetupPlayer(GameObject player)
    {
        player.transform.position = new Vector3(0f, -1.75f, 0f);
        player.transform.localScale = Vector3.one;
        PlaceholderSprites.MakeSquare(player, new Color(0.95f, 0.95f, 0.95f), 10);

        Rigidbody2D body = GetOrAdd<Rigidbody2D>(player);
        body.gravityScale = 4.5f;
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
    }

    private static void SetupDummy()
    {
        GameObject dummy = GameObject.Find("Enemy Dummy");
        if (dummy == null)
        {
            dummy = new GameObject("Enemy Dummy");
        }

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
