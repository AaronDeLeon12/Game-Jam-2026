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
    [SerializeField] private float triangleFullChargeTime = 0.8f;
    [SerializeField] private float triangleOverchargeTime = 4f;
    [SerializeField] private float triangleOverchargeSelfDamage = 20f;
    [SerializeField] private float triangleMinDamageAsSquarePercent = 0.55f;
    [SerializeField] private float triangleMinRangeAsSquarePercent = 0.75f;
    [SerializeField] private float triangleMaxRangeAsSquarePercent = 1.5f;
    [SerializeField] private float circleManaCost = 10f;
    [SerializeField] private float circleShieldHealth = 20f;
    [SerializeField] private float circleShieldDuration = 2f;
    [SerializeField] private float circleCooldown = 0.4f;
    [SerializeField] private float circleShieldScale = 4.2f;
    [SerializeField] private float knifeDamage = 10f;
    [SerializeField] private float knifeResourceValue = 20f;
    [SerializeField] private float knifeCooldown = 0.5f;
    [SerializeField] private float spellManaRegenDelay = 3f;

    private PlayerMovement2D movement;
    private PlayerStats stats;
    private PlayerActionCounter actionCounter;
    private SpriteRenderer playerVisualRenderer;
    private PlayerSpriteAnimator spriteAnimator;
    private float nextCastTime;
    private float lastCooldownDuration = 1f;
    private float triangleChargeStartTime;
    private bool isChargingTriangle;
    private SpellType equippedSpell = SpellType.Square;

    public SpellType EquippedSpell => equippedSpell;
    public Color EquippedSpellColor => GetSpellColor(equippedSpell);
    public float CooldownRemainingPercent => Time.time < nextCastTime
        ? Mathf.Clamp01((nextCastTime - Time.time) / Mathf.Max(0.01f, lastCooldownDuration))
        : 0f;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement2D>();
        stats = GetComponent<PlayerStats>();
        actionCounter = GetComponent<PlayerActionCounter>();
        playerVisualRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteAnimator = GetComponent<PlayerSpriteAnimator>();
    }

    private void Update()
    {
        if (PauseMenu.IsPaused || GameModal.IsOpen || HomeMode.IsActive || (stats != null && stats.IsDead))
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
        HandleTriangleCharge();

        if (equippedSpell == SpellType.Triangle)
        {
            return;
        }

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
            StartCooldown(GetCooldown(equippedSpell));
        }
    }

    private void LateUpdate()
    {
        if (isChargingTriangle)
        {
            UpdateTriangleChargeFlash(Time.time - triangleChargeStartTime);
        }
    }

    private void HandleSpellScroll()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f)
        {
            CancelTriangleCharge();
            equippedSpell = (SpellType)(((int)equippedSpell + 1) % 4);
        }
        else if (scroll < 0f)
        {
            CancelTriangleCharge();
            equippedSpell = (SpellType)(((int)equippedSpell + 3) % 4);
        }
    }

    private void HandleTriangleCharge()
    {
        if (equippedSpell != SpellType.Triangle)
        {
            CancelTriangleCharge();
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= nextCastTime && CanCastOffensiveSpell())
        {
            isChargingTriangle = true;
            triangleChargeStartTime = Time.time;
        }

        if (!isChargingTriangle)
        {
            return;
        }

        float chargeTime = Time.time - triangleChargeStartTime;
        UpdateTriangleChargeFlash(chargeTime);
        if (chargeTime >= triangleOverchargeTime)
        {
            OverchargeTriangle();
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseTriangle(chargeTime);
        }
    }

    private bool CanCastOffensiveSpell()
    {
        return stats != null
            && !stats.IsBlocking
            && GetComponentInChildren<CircleShield>() == null;
    }

    private void CancelTriangleCharge()
    {
        isChargingTriangle = false;
        triangleChargeStartTime = 0f;
        RestorePlayerVisualColor();
    }

    private void UpdateTriangleChargeFlash(float chargeTime)
    {
        if (playerVisualRenderer == null)
        {
            playerVisualRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (playerVisualRenderer == null)
        {
            return;
        }

        float chargePercent = Mathf.Clamp01(chargeTime / Mathf.Max(0.01f, triangleFullChargeTime));
        float dangerPercent = Mathf.Clamp01(chargeTime / Mathf.Max(0.01f, triangleOverchargeTime));
        float flashSpeed = Mathf.Lerp(6f, 16f, dangerPercent);
        float pulse = Mathf.PingPong(Time.time * flashSpeed, 1f);
        Color chargeColor = Color.Lerp(new Color(1f, 0.85f, 0.25f), new Color(1f, 0.25f, 0.08f), dangerPercent);
        float tintStrength = Mathf.Lerp(0.25f, 0.75f, Mathf.Max(chargePercent, dangerPercent));
        playerVisualRenderer.color = Color.Lerp(Color.white, chargeColor, pulse * tintStrength);
    }

    private void RestorePlayerVisualColor()
    {
        if (playerVisualRenderer == null)
        {
            playerVisualRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (playerVisualRenderer != null)
        {
            playerVisualRenderer.color = Color.white;
        }
    }

    private void ReleaseTriangle(float chargeTime)
    {
        CancelTriangleCharge();

        if (!CanCastOffensiveSpell())
        {
            return;
        }

        if (!stats.TryPayCost(triangleManaCost, spellManaRegenDelay))
        {
            return;
        }

        float chargePercent = Mathf.Clamp01(chargeTime / Mathf.Max(0.01f, triangleFullChargeTime));
        RecordAction(chargePercent >= 1f ? "spell_triangle_charged" : "spell_triangle_undercharged");
        CastProjectile(SpellType.Triangle, chargePercent);
        StartCooldown(triangleCooldown);
    }

    private void OverchargeTriangle()
    {
        CancelTriangleCharge();
        RecordAction("spell_triangle_overcharge");
        stats.TryPayCost(triangleManaCost, spellManaRegenDelay);
        stats.TakeDirectDamage(triangleOverchargeSelfDamage);
        StartCooldown(triangleCooldown);
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
            CastProjectile(equippedSpell, 1f);
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

    private void StartCooldown(float duration)
    {
        lastCooldownDuration = Mathf.Max(0.01f, duration);
        nextCastTime = Time.time + lastCooldownDuration;
    }

    private void CastProjectile(SpellType spellType, float chargePercent)
    {
        if (spellType == SpellType.Square || spellType == SpellType.Triangle)
        {
            if (spriteAnimator == null)
            {
                spriteAnimator = GetComponent<PlayerSpriteAnimator>();
            }

            spriteAnimator?.PlaySpellAttack();
        }

        int direction = movement != null ? movement.FacingDirection : 1;
        Vector3 spawnPosition = transform.position + new Vector3(direction * 0.75f, 0f, 0f);

        GameObject projectile = new GameObject("Spell Projectile");
        projectile.transform.position = spawnPosition;
        Vector3 visualScale = GetProjectileVisualScale(spellType, direction);
        projectile.transform.localScale = visualScale;
        SpriteRenderer renderer = projectile.AddComponent<SpriteRenderer>();
        Sprite[] animationFrames = spellType == SpellType.Triangle
            ? MagicAttackSprites.TriangleFrames
            : MagicAttackSprites.SquareFrames;
        renderer.sprite = MagicAttackSprites.FirstOrFallback(animationFrames, ShapeSprites.Get(spellType));
        renderer.color = animationFrames.Length > 0 ? Color.white : GetSpellColor(spellType);
        renderer.sortingOrder = 20;
        SpriteLit.Apply(renderer);

        if (animationFrames.Length > 1)
        {
            projectile.AddComponent<AnimatedSprite2D>().Play(animationFrames, 14f);
        }

        BoxCollider2D collider = projectile.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = GetProjectileColliderSize(spellType);

        Rigidbody2D body = projectile.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        SpellProjectile spell = projectile.AddComponent<SpellProjectile>();
        spell.Launch(direction, projectileSpeed, GetProjectileRange(spellType, chargePercent), GetDamage(spellType, chargePercent));
        GameAudio.PlaySfx(spellType == SpellType.Triangle ? "triangleSF" : "squareSF", transform.position, 0.85f);
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
        shieldObject.transform.localPosition = new Vector3(0.06f, 0f, 0f);
        shieldObject.transform.localScale = new Vector3(circleShieldScale * 0.62f, circleShieldScale * 0.62f, 1f);

        SpriteRenderer renderer = shieldObject.AddComponent<SpriteRenderer>();
        Sprite shieldSprite = MagicAttackSprites.ShieldSprite;
        renderer.sprite = shieldSprite != null ? shieldSprite : ShapeSprites.Circle;
        renderer.color = shieldSprite != null ? new Color(1f, 1f, 1f, 0.9f) : new Color(0.45f, 1f, 0.55f, 0.35f);
        SpriteLit.Apply(renderer);
        renderer.sortingOrder = 8;

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

    private float GetDamage(SpellType spellType, float chargePercent = 1f)
    {
        switch (spellType)
        {
            case SpellType.Triangle:
                float easedCharge = Mathf.Clamp01(chargePercent) * Mathf.Clamp01(chargePercent);
                float minDamage = squareDamage * triangleMinDamageAsSquarePercent;
                return Mathf.Lerp(minDamage, triangleDamage, easedCharge);
            default:
                return squareDamage;
        }
    }

    private float GetProjectileRange(SpellType spellType, float chargePercent)
    {
        if (spellType != SpellType.Triangle)
        {
            return projectileRange;
        }

        float rangePercent = Mathf.Lerp(
            triangleMinRangeAsSquarePercent,
            triangleMaxRangeAsSquarePercent,
            Mathf.Clamp01(chargePercent));
        return projectileRange * rangePercent;
    }

    private Vector3 GetProjectileVisualScale(SpellType spellType, int direction)
    {
        float xDirection = direction > 0 ? -1f : 1f;
        if (spellType == SpellType.Triangle)
        {
            return new Vector3(xDirection * 1.15f, 0.55f, 1f);
        }

        return new Vector3(xDirection * 1.35f, 1.25f, 1f);
    }

    private Vector2 GetProjectileColliderSize(SpellType spellType)
    {
        return spellType == SpellType.Triangle
            ? new Vector2(0.95f, 0.35f)
            : new Vector2(1.35f, 1.15f);
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
