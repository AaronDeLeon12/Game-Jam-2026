using UnityEngine;

/// <summary>
/// Level content for the "PlatformTesting" scene: a terrain sandbox built from
/// TerrainBlock templates so we can experiment with floor/platform/wall/obstacle.
/// Ensures the persistent systems (player/camera/HUD) exist, then lays out the
/// playground and a spawn point.
/// </summary>
public class PlatformTestLevel : MonoBehaviour
{
    private void Awake()
    {
        SystemsBootstrap.EnsureExists();

        // Spawn point on the far left of the floor.
        GameObject spawn = new GameObject("Player Spawn Point");
        spawn.transform.position = new Vector3(-20f, -1f, 0f);
        spawn.AddComponent<PlayerSpawnPoint>();

        // Base floor.
        TerrainBlock.Spawn(TerrainType.Floor, new Vector2(0f, -3f), new Vector2(52f, 1f), transform, "Floor");

        // One-way platforms at increasing heights (jump up through, land on top).
        TerrainBlock.Spawn(TerrainType.Platform, new Vector2(-12f, -1f), new Vector2(4f, 0.4f), transform, "Platform Low");
        TerrainBlock.Spawn(TerrainType.Platform, new Vector2(-6f, 0.6f), new Vector2(4f, 0.4f), transform, "Platform Mid");
        TerrainBlock.Spawn(TerrainType.Platform, new Vector2(0f, 2.2f), new Vector2(4f, 0.4f), transform, "Platform High");

        // A wall the player must jump over (cannot stand on its side).
        TerrainBlock.Spawn(TerrainType.Wall, new Vector2(6f, -1.5f), new Vector2(0.6f, 3f), transform, "Wall");

        // A solid obstacle / low step.
        TerrainBlock.Spawn(TerrainType.Obstacle, new Vector2(10f, -2f), new Vector2(2f, 1f), transform, "Obstacle Step");

        // A ceiling-style obstacle to duck under (S key).
        TerrainBlock.Spawn(TerrainType.Obstacle, new Vector2(15f, -0.6f), new Vector2(5f, 0.4f), transform, "Obstacle Ceiling");

        // Landing platform past the obstacles.
        TerrainBlock.Spawn(TerrainType.Platform, new Vector2(21f, 0.4f), new Vector2(4f, 0.4f), transform, "Platform End");

        SpawnEnemyTestArena();
    }

    private void SpawnEnemyTestArena()
    {
        TerrainBlock.Spawn(TerrainType.Floor, new Vector2(42f, -3f), new Vector2(34f, 1f), transform, "Enemy Test Floor");

        CreateEnemy("Flying Enemy Prototype", new Vector3(31f, 1.5f, 0f), Vector3.one, new Color(0.8f, 0.25f, 1f), 50f, true).AddComponent<FlyingEnemyAI>();
        CreateEnemy("Small Contact Enemy Prototype", new Vector3(36f, -2.25f, 0f), new Vector3(0.65f, 0.65f, 1f), new Color(1f, 0.25f, 0.25f), 80f, false).AddComponent<SmallContactEnemyAI>();
        CreateEnemy("Heavy Enemy Prototype", new Vector3(43f, -1f, 0f), new Vector3(3f, 3f, 1f), new Color(0.55f, 0.1f, 0.75f), 300f, false).AddComponent<HeavyEnemyAI>();
        CreateEnemy("Kiting Shooter Enemy Prototype", new Vector3(53f, -1.75f, 0f), Vector3.one, new Color(0.1f, 0.75f, 0.25f), 100f, false).AddComponent<KitingShooterEnemyAI>();
    }

    private static GameObject CreateEnemy(string name, Vector3 position, Vector3 scale, Color color, float health, bool isTrigger)
    {
        GameObject enemy = new GameObject(name);
        enemy.transform.position = position;
        enemy.transform.localScale = scale;
        PlaceholderSprites.MakeSquare(enemy, color, 10);

        BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
        collider.isTrigger = isTrigger;
        collider.size = Vector2.one;

        Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
        body.gravityScale = isTrigger ? 0f : 5f;
        body.freezeRotation = true;

        EnemyHealth2D enemyHealth = enemy.AddComponent<EnemyHealth2D>();
        enemyHealth.Configure(health);
        return enemy;
    }
}
