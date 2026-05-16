using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 13f;
    [SerializeField] private float projectileRange = 3.5f;
    [SerializeField] private float squareManaCost = 20f;
    [SerializeField] private float squareDamage = 1f;
    [SerializeField] private float squareCooldown = 0.8f;
    [SerializeField] private float triangleManaCost = 30f;
    [SerializeField] private float triangleDamage = 3f;
    [SerializeField] private float triangleCooldown = 2f;
    [SerializeField] private float circleManaCost = 10f;
    [SerializeField] private float circleDamage = 0.5f;
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
            CastProjectile();
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
            case SpellType.Circle:
                return circleDamage;
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
