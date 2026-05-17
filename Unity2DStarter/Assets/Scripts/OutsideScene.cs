using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Level content for "outside_1". Ensures day state and game systems exist,
/// places a spawn point and a basic ground so the area is playable.
/// (HomeMode is automatically cleared when leaving the home scene, so the
/// HUD and full player abilities return here.)
///
/// Day progression (driven by DayManager.CurrentDay):
///  - Day 1: nothing spawns, the player just explores.
///  - Day 2: one mantis that attacks the player.
///  - Day 3: one unicorn + one mantis.
///  - Day 4: one fairy + one unicorn + one mantis (all chase the player).
///  - Day 5+: the player is taken to the boss fight scene instead of here.
/// </summary>
public class OutsideScene : MonoBehaviour
{
    private const string BossScene = "boss_fight_scenario";
    private const int BossDay = 5;

    private void Awake()
    {
        DayManager.EnsureExists();

        // Day 5+: this area is over -> go straight to the boss fight scene.
        int currentDay = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;
        if (currentDay >= BossDay)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(BossScene);
            }
            else
            {
                SceneManager.LoadScene(BossScene);
            }
            return;
        }

        // Bright neutral daytime light. Created before EnsureExists() so the
        // systems' dim default global light is skipped (it only adds one when
        // no global light already exists), avoiding two lights stacking.
        BuildDaylight();

        SystemsBootstrap.EnsureExists();

        // The camera is persistent; reset the zoom in case we came from the
        // home scene (which zooms in).
        Camera cam = SystemsBootstrap.Instance != null && SystemsBootstrap.Instance.GameCamera != null
            ? SystemsBootstrap.Instance.GameCamera
            : Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = 5f;
            CameraFollow2D follow = cam.GetComponent<CameraFollow2D>();
            if (follow != null)
            {
                follow.SetOffset(new Vector3(0f, 2.5f, -10f));
            }
        }

        SceneBackground.Show("Backgrounds/fondo_verde");

        GameObject spawn = new GameObject("Player Spawn Point");
        spawn.transform.position = new Vector3(-8f, 0.5f, 0f);
        spawn.AddComponent<PlayerSpawnPoint>();

        BuildSaveVendorArea();
        BuildReturnDoor();
        BuildDayEnemies(currentDay);

        // Floor is now a hand-placed scene object ("Floor" with TerrainBlock)
        // in outside_1, so it is not generated here anymore.
    }

    // Spawns the enemies for the current day. See the class summary for the
    // per-day plan. Ground enemies have gravity and fall onto the floor;
    // the fairy flies. They spawn to the right of the player spawn (-8) so
    // they advance toward the player.
    private void BuildDayEnemies(int day)
    {
        switch (day)
        {
            case 1:
                // Nothing: free exploration.
                break;

            case 2:
                SpawnMantis(new Vector3(4f, 1.5f, 0f));
                break;

            case 3:
                SpawnUnicorn(new Vector3(8f, 1.5f, 0f));
                SpawnMantis(new Vector3(3f, 1.5f, 0f));
                break;

            default: // day 4 (day 5+ never reaches here, it redirects)
                SpawnFairy(new Vector3(6f, 4f, 0f));
                SpawnUnicorn(new Vector3(9f, 1.5f, 0f));
                SpawnMantis(new Vector3(3f, 1.5f, 0f));
                break;
        }
    }

    private void SpawnMantis(Vector3 position)
    {
        GameObject m = new GameObject("Mantis");
        m.transform.SetParent(transform, false);
        m.transform.position = position;

        Rigidbody2D body = m.AddComponent<Rigidbody2D>();
        body.gravityScale = 2f;
        body.freezeRotation = true;

        BoxCollider2D col = m.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.size = new Vector2(1f, 1.4f);

        // MantisEnemy [RequireComponent]s MantisAnimator, so this adds both.
        m.AddComponent<MantisEnemy>();
    }

    private void SpawnUnicorn(Vector3 position)
    {
        GameObject u = new GameObject("Unicorn");
        u.transform.SetParent(transform, false);
        u.transform.position = position;

        Rigidbody2D body = u.AddComponent<Rigidbody2D>();
        body.gravityScale = 5f;
        body.freezeRotation = true;

        BoxCollider2D col = u.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.size = new Vector2(1.2f, 1.6f);

        u.AddComponent<EnemyHealth2D>().Configure(100f);
        u.AddComponent<KitingShooterEnemyAI>();
    }

    private void SpawnFairy(Vector3 position)
    {
        GameObject f = new GameObject("Fairy");
        f.transform.SetParent(transform, false);
        f.transform.position = position;

        // FlyingEnemyAI sets the body kinematic and the collider as a trigger
        // itself in Awake; it just needs them to exist.
        f.AddComponent<Rigidbody2D>();
        BoxCollider2D col = f.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);

        f.AddComponent<EnemyHealth2D>().Configure(50f);
        f.AddComponent<FlyingEnemyAI>();
    }

    private void BuildSaveVendorArea()
    {
        Vector3 housePosition = new Vector3(14f, 0.65f, 0f);

        GameObject vendorHouse = new GameObject("Casa del Vendedor Completa");
        vendorHouse.transform.SetParent(transform, false);
        vendorHouse.transform.position = housePosition;

        SpriteRenderer houseRenderer = vendorHouse.AddComponent<SpriteRenderer>();
        houseRenderer.sprite = RuntimeSpriteCropper.LoadTrimmedSprite("Home/completeHouse", 256f, 6);
        houseRenderer.sortingOrder = 4;
        SpriteLit.Apply(houseRenderer);
        if (houseRenderer.sprite != null)
        {
            float scale = 5.8f / Mathf.Max(0.01f, houseRenderer.sprite.bounds.size.y);
            vendorHouse.transform.localScale = new Vector3(scale, scale, 1f);
        }

        GameObject vendor = new GameObject("Vendedor Save Machine");
        vendor.transform.SetParent(transform, false);
        vendor.transform.position = new Vector3(14f, 1.1f, 0f);
        vendor.AddComponent<SpriteRenderer>();
        vendor.AddComponent<VendorSpriteAnimator>();

        BoxCollider2D collider = vendor.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.6f, 2.2f);
        collider.isTrigger = true;
        vendor.AddComponent<SaveVendor>();

    }

    private void BuildReturnDoor()
    {
        GameObject door = new GameObject("Door Back To House");
        door.transform.SetParent(transform, false);
        door.transform.position = new Vector3(-10.5f, -1.4f, 0f);
        door.transform.localScale = new Vector3(1.1f, 2f, 1f);

        SpriteRenderer renderer = PlaceholderSprites.MakeSquare(door, new Color(0.38f, 0.22f, 0.11f), 4);
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = Vector2.one;

        BoxCollider2D collider = door.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = Vector2.one;

        DoorInteractable interactable = door.AddComponent<DoorInteractable>();
        interactable.Configure("home_day_1", "Press E to go home", "Go back inside the house?");
    }

    private void BuildDaylight()
    {
        SceneLighting.ReplaceGlobalLight("Daylight Global Light", Color.white, 1f);
    }
}
