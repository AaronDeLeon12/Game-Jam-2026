using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileRange = 3f;
    [SerializeField] private float castCooldown = 0.6f;

    private PlayerMovement2D movement;
    private PlayerStats stats;
    private float nextCastTime;

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

        if (Input.GetMouseButtonDown(1) && Time.time >= nextCastTime && stats.TryPaySpellCost())
        {
            CastProjectile();
            nextCastTime = Time.time + castCooldown;
        }
    }

    private void CastProjectile()
    {
        int direction = movement != null ? movement.FacingDirection : 1;
        Vector3 spawnPosition = transform.position + new Vector3(direction * 0.75f, 0f, 0f);

        GameObject projectile = new GameObject("Spell Projectile");
        projectile.transform.position = spawnPosition;
        projectile.transform.localScale = Vector3.one;
        PlaceholderSprites.MakeSquare(projectile, new Color(0.35f, 0.75f, 1f), 20);

        BoxCollider2D collider = projectile.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Rigidbody2D body = projectile.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        SpellProjectile spell = projectile.AddComponent<SpellProjectile>();
        spell.Launch(direction, projectileSpeed, projectileRange);
    }
}
