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
    [SerializeField] private float circleShieldHealth = 20f;
    [SerializeField] private float circleShieldDuration = 3f;
    [SerializeField] private float circleCooldown = 0.4f;
    [SerializeField] private float circleShieldScale = 4.2f;
    [SerializeField] private float knifeDamage = 10f;
    [SerializeField] private float knifeResourceValue = 20f;
    [SerializeField] private float knifeCooldown = 0.5f;
    [SerializeField] private float spellManaRegenDelay = 3f;

    private PlayerMovement2D movement;
    private PlayerStats stats;
    private PlayerActionCounter actionCounter;
    private float nextCastTime;
    private SpellType equippedSpell = SpellType.Square;

    public SpellType EquippedSpell => equippedSpell;
    public Color EquippedSpellColor => GetSpellColor(equippedSpell);

    private void Awake()
    {
        movement = GetComponent<PlayerMovement2D>();
        stats = GetComponent<PlayerStats>();
        actionCounter = GetComponent<PlayerActionCounter>();
    }

    private void Update()
    {
        if (PauseMenu.IsPaused || HomeMode.IsActive)
        {
            return;
        }

        if (stats == null)
        {
            stats = GetComponent<PlayerStats>();
        }

        if (stats == null)
        {
            return;
        }

        HandleSpellScroll();

        if (Input.GetMouseButtonDown(0) && Time.time >= nextCastTime)
        {
            if (stats.IsBlocking)
            {
                return;
            }

            if (GetComponentInChildren<CircleShield>() != null)
            {
                return;
            }

            if (RequiresManaPayment(equippedSpell) && !stats.TryPayCost(GetManaCost(equippedSpell), spellManaRegenDelay))
            {
                return;
            }

            CastEquippedSpell();
            nextCastTime = Time.time + GetCooldown(equippedSpell);
        }
    }

    private void HandleSpellScroll()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f)
        {
            equippedSpell = (SpellType)(((int)equippedSpell + 1) % 4);
        }
        else if (scroll < 0f)
        {
            equippedSpell = (SpellType)(((int)equippedSpell + 3) % 4);
        }
    }

    private void CastEquippedSpell()
    {
        if (equippedSpell == SpellType.Circle)
        {
            RecordAction("spell_circle_shield");
            CastCircleShield();
        }
        else if (equippedSpell == SpellType.Knife)
        {
            RecordAction("attack_knife");
            CastKnifeAttack();
        }
        else
        {
            RecordAction(equippedSpell == SpellType.Triangle ? "spell_triangle" : "spell_square");
            CastProjectile();
        }
    }

    private void RecordAction(string actionName)
    {
        if (actionCounter == null)
        {
            actionCounter = GetComponent<PlayerActionCounter>();
        }

        if (actionCounter != null)
        {
            actionCounter.Record(actionName);
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
        SpriteLit.Apply(renderer);

        BoxCollider2D collider = projectile.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Rigidbody2D body = projectile.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        SpellProjectile spell = projectile.AddComponent<SpellProjectile>();
        spell.Launch(direction, projectileSpeed, projectileRange, GetDamage(equippedSpell));
        GameAudio.PlaySfx(equippedSpell == SpellType.Triangle ? "triangleSF" : "squareSF", transform.position, 0.85f);
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
        shieldObject.transform.localScale = new Vector3(circleShieldScale, circleShieldScale, 1f);

        SpriteRenderer renderer = shieldObject.AddComponent<SpriteRenderer>();
        renderer.sprite = ShapeSprites.Circle;
        renderer.color = new Color(0.45f, 1f, 0.55f, 0.35f);
        SpriteLit.Apply(renderer);
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
        GameAudio.PlaySfx("shieldOnSF", transform.position, 0.85f);
    }

    private void CastKnifeAttack()
    {
        int direction = movement != null ? movement.FacingDirection : 1;
        Vector3 spawnPosition = transform.position + new Vector3(direction * 0.85f, 0f, 0f);

        GameObject knifeObject = new GameObject("Knife Attack");
        knifeObject.transform.position = spawnPosition;
        knifeObject.transform.localScale = new Vector3(direction, 1f, 1f);

        SpriteRenderer renderer = knifeObject.AddComponent<SpriteRenderer>();
        renderer.sprite = ShapeSprites.Knife;
        renderer.color = GetSpellColor(SpellType.Knife);
        SpriteLit.Apply(renderer);
        renderer.sortingOrder = 22;

        BoxCollider2D collider = knifeObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = Vector2.one;

        Rigidbody2D body = knifeObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        MeleeAttack2D attack = knifeObject.AddComponent<MeleeAttack2D>();
        attack.Launch(gameObject, knifeDamage, knifeResourceValue, 0.15f);
        HitFlash2D.Play(knifeObject, Color.white, 0.06f);
        GameAudio.PlaySfx("knifeMissSFX", transform.position, 0.8f);
    }

    private float GetManaCost(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Triangle:
                return triangleManaCost;
            case SpellType.Circle:
                return circleManaCost;
            case SpellType.Knife:
                return 0f;
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
            case SpellType.Knife:
                return knifeCooldown;
            default:
                return squareCooldown;
        }
    }

    private static bool RequiresManaPayment(SpellType spellType)
    {
        return spellType != SpellType.Knife;
    }

    private static Color GetSpellColor(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Triangle:
                return new Color(1f, 0.72f, 0.1f);
            case SpellType.Circle:
                return new Color(0.45f, 1f, 0.55f);
            case SpellType.Knife:
                return new Color(0.95f, 0.95f, 1f);
            default:
                return new Color(0.35f, 0.75f, 1f);
        }
    }
}
