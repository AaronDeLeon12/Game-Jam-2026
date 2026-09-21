using UnityEngine;

public static class MantisEnemyFactory
{
    public const string DefaultName = "Mantis";

    public static GameObject Create(Vector3 position, Transform parent = null, string name = DefaultName)
    {
        GameObject mantis = new GameObject(string.IsNullOrEmpty(name) ? DefaultName : name);
        Configure(mantis, position, parent);
        return mantis;
    }

    public static void Configure(GameObject mantis, Vector3 position, Transform parent = null)
    {
        mantis.name = string.IsNullOrEmpty(mantis.name) ? DefaultName : mantis.name;
        mantis.transform.SetParent(parent, false);
        mantis.transform.position = position;
        mantis.transform.localScale = Vector3.one;

        if (mantis.GetComponent<EditorPlaceable>() == null)
        {
            mantis.AddComponent<EditorPlaceable>();
        }

        Rigidbody2D body = GetOrAdd<Rigidbody2D>(mantis);
        body.gravityScale = 2f;
        body.freezeRotation = true;

        BoxCollider2D collider = GetOrAdd<BoxCollider2D>(mantis);
        collider.isTrigger = false;
        collider.size = new Vector2(1f, 1.4f);

        GetOrAdd<MantisEnemy>(mantis);
        GetOrAdd<MantisAnimator>(mantis);
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
