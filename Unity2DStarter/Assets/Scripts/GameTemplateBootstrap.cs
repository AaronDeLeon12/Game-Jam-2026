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

        GetOrAdd<HealthBar>(dummy);
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
