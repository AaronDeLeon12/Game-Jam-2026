using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SurvivalTestingScene : MonoBehaviour
{
    [SerializeField] private float chunkWidth = 28f;
    [SerializeField] private float chunkHeight = 16f;
    [SerializeField] private int generationRadius = 1;
    [SerializeField] private float enemyActivationRange = 18f;

    private readonly HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();
    private Transform player;
    private Transform chunkRoot;

    private enum EnemyKind
    {
        BasicChaser,
        Flying,
        SmallContact,
        KitingShooter
    }

    private void Awake()
    {
        HomeMode.IsActive = false;
        SystemsBootstrap.EnsureExists();
        BuildFlatLight();
        BuildSpawnPoint();
        ConfigureCamera();
        GameAudio.PlayMusic("MainMenu", 0.35f);

        chunkRoot = new GameObject("Survival Generated Chunks").transform;

        if (SystemsBootstrap.Instance != null && SystemsBootstrap.Instance.Player != null)
        {
            player = SystemsBootstrap.Instance.Player.transform;
            player.position = Vector3.zero;
        }
    }

    private void Start()
    {
        FindPlayer();
        GenerateAroundPlayer();
    }

    private void Update()
    {
        if (PauseMenu.IsPaused)
        {
            return;
        }

        FindPlayer();
        GenerateAroundPlayer();
    }

    private void GenerateAroundPlayer()
    {
        if (player == null)
        {
            return;
        }

        Vector2Int center = WorldToChunk(player.position);
        for (int y = center.y - generationRadius; y <= center.y + generationRadius; y++)
        {
            for (int x = center.x - generationRadius; x <= center.x + generationRadius; x++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                if (!generatedChunks.Contains(coord))
                {
                    generatedChunks.Add(coord);
                    GenerateChunk(coord);
                }
            }
        }
    }

    private void GenerateChunk(Vector2Int coord)
    {
        Transform chunk = new GameObject("Survival Chunk " + coord.x + "," + coord.y).transform;
        chunk.SetParent(chunkRoot, false);

        Vector2 origin = new Vector2(coord.x * chunkWidth, coord.y * chunkHeight);
        int seed = coord.x * 73856093 ^ coord.y * 19349663;
        Random.InitState(seed);

        bool isStartingChunk = coord == Vector2Int.zero;
        GenerateTerrain(coord, origin, chunk, isStartingChunk);

        if (!isStartingChunk && coord.y == 0)
        {
            GenerateEnemies(origin, chunk, seed);
        }
    }

    private void GenerateTerrain(Vector2Int coord, Vector2 origin, Transform parent, bool isStartingChunk)
    {
        if (coord.y == 0)
        {
            TerrainBlock floor = TerrainBlock.Spawn(
                TerrainType.Floor,
                new Vector2(origin.x + chunkWidth * 0.5f, -3f),
                new Vector2(chunkWidth + 1f, 1f),
                parent,
                "Survival Ground");
            TintTerrain(floor, new Color(0.24f, 0.25f, 0.29f));
        }

        int platformPattern = Mathf.Abs(coord.x * 17 + coord.y * 31) % 5;
        switch (platformPattern)
        {
            case 0:
                SpawnPlatform(parent, origin + new Vector2(7f, 0.2f), new Vector2(5f, 0.35f));
                SpawnPlatform(parent, origin + new Vector2(18f, 2.3f), new Vector2(4f, 0.35f));
                break;
            case 1:
                SpawnPlatform(parent, origin + new Vector2(8f, 1.2f), new Vector2(4f, 0.35f));
                SpawnPlatform(parent, origin + new Vector2(14f, 3.1f), new Vector2(4f, 0.35f));
                SpawnPlatform(parent, origin + new Vector2(21f, 1.6f), new Vector2(5f, 0.35f));
                break;
            case 2:
                SpawnPlatform(parent, origin + new Vector2(10f, -0.4f), new Vector2(6f, 0.35f));
                SpawnPlatform(parent, origin + new Vector2(20f, 1.5f), new Vector2(3.5f, 0.35f));
                break;
            case 3:
                SpawnPlatform(parent, origin + new Vector2(6f, 2.4f), new Vector2(3.5f, 0.35f));
                SpawnPlatform(parent, origin + new Vector2(15f, 0.7f), new Vector2(6f, 0.35f));
                SpawnPlatform(parent, origin + new Vector2(23f, 3.2f), new Vector2(3f, 0.35f));
                break;
            default:
                SpawnPlatform(parent, origin + new Vector2(13f, 1.4f), new Vector2(7f, 0.35f));
                break;
        }

        if (isStartingChunk)
        {
            SpawnPlatform(parent, new Vector2(5f, -0.5f), new Vector2(5f, 0.35f));
            SpawnPlatform(parent, new Vector2(-6f, 1.2f), new Vector2(4f, 0.35f));
        }
    }

    private void GenerateEnemies(Vector2 origin, Transform parent, int seed)
    {
        Random.InitState(seed + 91);
        int enemyCount = Random.Range(2, 5);

        for (int i = 0; i < enemyCount; i++)
        {
            float x = origin.x + Random.Range(4f, chunkWidth - 4f);
            float y = origin.y + Random.Range(-1.95f, 3.5f);
            EnemyKind kind = (EnemyKind)Random.Range(0, 4);
            Vector3 position = new Vector3(x, y, 0f);

            if (kind == EnemyKind.Flying)
            {
                position.y += 2f;
            }

            CreateEnemy(parent, kind, position, i);
        }
    }

    private void CreateEnemy(Transform parent, EnemyKind kind, Vector3 position, int index)
    {
        GameObject enemy = new GameObject("Survival " + kind + " " + index);
        enemy.transform.SetParent(parent, false);
        enemy.transform.position = position;

        bool flying = kind == EnemyKind.Flying;
        float health = 100f;
        Vector3 scale = Vector3.one;
        Color color = new Color(0.9f, 0.25f, 0.25f);

        switch (kind)
        {
            case EnemyKind.Flying:
                health = 50f;
                color = new Color(0.8f, 0.25f, 1f);
                break;
            case EnemyKind.SmallContact:
                health = 80f;
                scale = new Vector3(0.65f, 0.65f, 1f);
                color = new Color(1f, 0.25f, 0.25f);
                break;
            case EnemyKind.KitingShooter:
                health = 100f;
                color = new Color(0.1f, 0.75f, 0.25f);
                break;
            default:
                health = 100f;
                color = new Color(0.85f, 0.25f, 0.2f);
                break;
        }

        enemy.transform.localScale = scale;
        PlaceholderSprites.MakeSquare(enemy, color, 10);

        BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
        collider.isTrigger = flying;
        collider.size = Vector2.one;

        Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
        body.gravityScale = flying ? 0f : 5f;
        body.freezeRotation = true;
        if (flying)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
        }

        switch (kind)
        {
            case EnemyKind.Flying:
                enemy.AddComponent<EnemyHealth2D>().Configure(health);
                enemy.AddComponent<FlyingEnemyAI>();
                break;
            case EnemyKind.SmallContact:
                enemy.AddComponent<EnemyHealth2D>().Configure(health);
                enemy.AddComponent<SmallContactEnemyAI>();
                break;
            case EnemyKind.KitingShooter:
                enemy.AddComponent<EnemyHealth2D>().Configure(health);
                enemy.AddComponent<KitingShooterEnemyAI>();
                break;
            default:
                enemy.AddComponent<Enemy>();
                break;
        }

        SurvivalEnemyActivator activator = enemy.AddComponent<SurvivalEnemyActivator>();
        activator.Configure(enemyActivationRange);
    }

    private static void SpawnPlatform(Transform parent, Vector2 position, Vector2 size)
    {
        TerrainBlock platform = TerrainBlock.Spawn(TerrainType.Platform, position, size, parent, "Survival One-Way Platform");
        TintTerrain(platform, new Color(0.43f, 0.47f, 0.55f));
    }

    private static void TintTerrain(TerrainBlock terrain, Color color)
    {
        SpriteRenderer renderer = terrain.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
        }
    }

    private Vector2Int WorldToChunk(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / chunkWidth),
            Mathf.FloorToInt(position.y / chunkHeight));
    }

    private void FindPlayer()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private static void BuildSpawnPoint()
    {
        GameObject spawn = new GameObject("Player Spawn Point");
        spawn.transform.position = Vector3.zero;
        spawn.AddComponent<PlayerSpawnPoint>();
    }

    private static void ConfigureCamera()
    {
        if (SystemsBootstrap.Instance == null || SystemsBootstrap.Instance.GameCamera == null)
        {
            return;
        }

        Camera cam = SystemsBootstrap.Instance.GameCamera;
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.06f, 0.07f, 0.1f);

        CameraFollow2D follow = cam.GetComponent<CameraFollow2D>();
        if (follow != null)
        {
            follow.SetOffset(new Vector3(0f, 1f, -10f));
        }
    }

    private static void BuildFlatLight()
    {
        foreach (Light2D existing in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
        {
            if (existing.lightType == Light2D.LightType.Global)
            {
                existing.color = Color.white;
                existing.intensity = 0.85f;
                return;
            }
        }

        GameObject lightObj = new GameObject("Survival Flat Light");
        Light2D light = lightObj.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.color = Color.white;
        light.intensity = 0.85f;
    }
}
