using UnityEngine;

public class GameTemplateBootstrap : MonoBehaviour
{
    private void Awake()
    {
        SystemsBootstrap.EnsureExists();

        SetupFloor();
        SetupDummy();
        SetupMovementTestArea();
        SetupShooter();
        SetupEnemyAITestArea();
        RemoveOldPlatforms();
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
        PlaceholderSprites.MakeSquare(dummy, new Color(0.85f, 0.25f, 0.2f), 10);

        BoxCollider2D collider = GetOrAdd<BoxCollider2D>(dummy);
        collider.isTrigger = false;
        collider.size = Vector2.one;

        Rigidbody2D body = GetOrAdd<Rigidbody2D>(dummy);
        body.gravityScale = 5f;
        body.freezeRotation = true;

        GetOrAdd<Enemy>(dummy);
    }

    private static void SetupMovementTestArea()
    {
        CreateSolidBlock("Beta Left Step Platform", new Vector3(-6f, -2.15f, 0f), new Vector3(3f, 0.35f, 1f), new Color(0.42f, 0.45f, 0.5f), true);
        CreateSolidBlock("Beta Left Tall Obstacle", new Vector3(-9f, -2f, 0f), new Vector3(0.75f, 2f, 1f), new Color(0.5f, 0.42f, 0.35f), true);
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

        EnemyHealth2D shooterHealth = shooter.GetComponent<EnemyHealth2D>();
        if (shooterHealth == null)
        {
            shooterHealth = shooter.AddComponent<EnemyHealth2D>();
        }

        shooterHealth.Configure(100f);

        if (shooter.GetComponent<EnemyShooter>() == null)
        {
            shooter.AddComponent<EnemyShooter>();
        }
    }

    private static void SetupEnemyAITestArea()
    {
        CreateFlyingEnemy("Beta Flying Enemy", new Vector3(26f, 1.5f, 0f));
        CreateSmallContactEnemy("Beta Small Contact Enemy", new Vector3(32f, -2.25f, 0f));
        CreateHeavyEnemy("Beta Heavy Enemy", new Vector3(39f, -0.5f, 0f));
        CreateKitingShooterEnemy("Beta Kiting Shooter Enemy", new Vector3(48f, -1.75f, 0f));
    }

    private static void CreateFlyingEnemy(string name, Vector3 position)
    {
        GameObject enemy = CreateEnemyBase(name, position, Vector3.one, new Color(0.8f, 0.25f, 1f), 50f, true);
        GetOrAdd<FlyingEnemyAI>(enemy);
    }

    private static void CreateHeavyEnemy(string name, Vector3 position)
    {
        GameObject enemy = CreateEnemyBase(name, position, new Vector3(3f, 3f, 1f), new Color(0.55f, 0.1f, 0.75f), 300f, false);
        GetOrAdd<HeavyEnemyAI>(enemy);
    }

    private static void CreateSmallContactEnemy(string name, Vector3 position)
    {
        GameObject enemy = CreateEnemyBase(name, position, new Vector3(0.65f, 0.65f, 1f), new Color(1f, 0.25f, 0.25f), 80f, false);
        GetOrAdd<SmallContactEnemyAI>(enemy);
    }

    private static void CreateKitingShooterEnemy(string name, Vector3 position)
    {
        GameObject enemy = CreateEnemyBase(name, position, Vector3.one, new Color(0.1f, 0.75f, 0.25f), 100f, false);
        GetOrAdd<KitingShooterEnemyAI>(enemy);
    }

    private static GameObject CreateEnemyBase(string name, Vector3 position, Vector3 scale, Color color, float health, bool isTrigger)
    {
        GameObject enemy = GameObject.Find(name);
        if (enemy == null)
        {
            enemy = new GameObject(name);
        }

        enemy.transform.position = position;
        enemy.transform.localScale = scale;
        PlaceholderSprites.MakeSquare(enemy, color, 10);

        BoxCollider2D collider = GetOrAdd<BoxCollider2D>(enemy);
        collider.isTrigger = isTrigger;
        collider.size = Vector2.one;

        Rigidbody2D body = GetOrAdd<Rigidbody2D>(enemy);
        body.gravityScale = isTrigger ? 0f : 5f;
        body.freezeRotation = true;

        EnemyHealth2D enemyHealth = GetOrAdd<EnemyHealth2D>(enemy);
        enemyHealth.Configure(health);
        return enemy;
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
