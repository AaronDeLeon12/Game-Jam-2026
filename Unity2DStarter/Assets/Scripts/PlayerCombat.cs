using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 13f;
    [SerializeField] private float projectileRange = 3.5f;
    [SerializeField] private float squareManaCost = 20f;
    [SerializeField] private float squareDamage = 33.4f;
    [SerializeField] private float squareCooldown = 0.8f;
    [SerializeField] private float triangleManaCost = 30f;
    [SerializeField] private float triangleDamage = 100f;
    [SerializeField] private float triangleCooldown = 2f;
    [SerializeField] private float circleManaCost = 10f;
    [SerializeField] private float circleShieldHealth = 50f;
    [SerializeField] private float circleShieldDuration = 3f;
    [SerializeField] private float circleCooldown = 0.4f;
    [SerializeField] private float spellManaRegenDelay = 3f;

    private PlayerMovement2D movement;
    private PlayerStats stats;
    private float nextCastTime;
    private SpellType equippedSpell = SpellType.Square;

    public SpellType EquippedSpell => equippedSpell;
    public Color EquippedSpellColor => GetSpellColor(equippedSpell);

    private void Awake()
    {
        movement = GetComponent<PlayerMovement2D>();
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (stats == null)
        {
            stats = GetComponent<PlayerStats>();
        }

        if (stats == null)
        {
            return;
        }

        HandleSpellScroll();

        if (Input.GetMouseButtonDown(0) && Time.time >= nextCastTime && stats.TryPayCost(GetManaCost(equippedSpell), spellManaRegenDelay))
        {
            CastEquippedSpell();
            nextCastTime = Time.time + GetCooldown(equippedSpell);
        }
    }

    private void HandleSpellScroll()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f)
        {
            equippedSpell = (SpellType)(((int)equippedSpell + 1) % 3);
        }
        else if (scroll < 0f)
        {
            equippedSpell = (SpellType)(((int)equippedSpell + 2) % 3);
        }
    }

    private void CastEquippedSpell()
    {
        if (equippedSpell == SpellType.Circle)
        {
            CastCircleShield();
        }
        else
        {
            CastProjectile();
        }
    }

    private void CastProjectile()
    {
        int direction = movement != null ? movement.FacingDirection : 1;
        Vector3 spawnPosition = transform.position + new Vector3(direction * 0.75f, 0f, 0f);

        GameObject projectile = new GameObject("Spell Projectile");
        projectile.transform.position = spawnPosition;
        projectile.transform.localScale = Vector3.one;
        SpriteRenderer renderer = projectile.AddComponent<SpriteRenderer>();
        renderer.sprite = ShapeSprites.Get(equippedSpell);
        renderer.color = GetSpellColor(equippedSpell);
        renderer.sortingOrder = 20;

        BoxCollider2D collider = projectile.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Rigidbody2D body = projectile.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        SpellProjectile spell = projectile.AddComponent<SpellProjectile>();
        spell.Launch(direction, projectileSpeed, projectileRange, GetDamage(equippedSpell));
    }

    private void CastCircleShield()
    {
        CircleShield existingShield = GetComponentInChildren<CircleShield>();
        if (existingShield != null)
        {
            Destroy(existingShield.gameObject);
        }

        GameObject shieldObject = new GameObject("Circle Shield");
        shieldObject.transform.SetParent(transform, false);
        shieldObject.transform.localPosition = Vector3.zero;
        shieldObject.transform.localScale = new Vector3(4.8f, 4.8f, 1f);

        SpriteRenderer renderer = shieldObject.AddComponent<SpriteRenderer>();
        renderer.sprite = ShapeSprites.Circle;
        renderer.color = new Color(0.45f, 1f, 0.55f, 0.35f);
        renderer.sortingOrder = 15;

        CircleCollider2D collider = shieldObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = false;
        collider.radius = 0.5f;

        Rigidbody2D body = shieldObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(collider, playerCollider, true);
        }

        CircleShield shield = shieldObject.AddComponent<CircleShield>();
        shield.Activate(circleShieldHealth, circleShieldDuration);
    }

    private float GetManaCost(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Triangle:
                return triangleManaCost;
            case SpellType.Circle:
                return circleManaCost;
            default:
                return squareManaCost;
        }
    }

    private float GetDamage(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Triangle:
                return triangleDamage;
            default:
                return squareDamage;
        }
    }

    private float GetCooldown(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Triangle:
                return triangleCooldown;
            case SpellType.Circle:
                return circleCooldown;
            default:
                return squareCooldown;
        }
    }

    private static Color GetSpellColor(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Triangle:
                return new Color(1f, 0.72f, 0.1f);
            case SpellType.Circle:
                return new Color(0.45f, 1f, 0.55f);
            default:
                return new Color(0.35f, 0.75f, 1f);
        }
    }
}
