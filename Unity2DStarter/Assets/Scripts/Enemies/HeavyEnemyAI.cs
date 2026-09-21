using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class HeavyEnemyAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float knifeRange = 2f;
    [SerializeField] private float triangleRange = 4f;
    [SerializeField] private float circleRange = 5.5f;
    [SerializeField] private float knifeDamage = 25f;
    [SerializeField] private float triangleDamage = 35f;
    [SerializeField] private float circleDamage = 10f;
    [SerializeField] private float circleAttackCooldown = 2f;
    [SerializeField] private float attackCooldown = 2.2f;
    [SerializeField] private float windupDuration = 0.5f;
    [SerializeField] private float hitboxDuration = 0.18f;
    [SerializeField] private float recoveryDuration = 1.1f;

    private Rigidbody2D body;
    private Transform player;
    private int facingDirection = 1;
    private float nextAttackTime;
    private float nextCircleAttackTime;
    private float stateEndTime;
    private State state;
    private AttackType queuedAttack;

    private enum State
    {
        Chase,
        Windup,
        Attack,
        Recover
    }

    private enum AttackType
    {
        Knife,
        Triangle,
        Circle
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 5f;
        body.freezeRotation = true;
        FindPlayer();
    }

    private void Update()
    {
        if (PauseMenu.IsPaused)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        if (player == null)
        {
            FindPlayer();
            return;
        }

        switch (state)
        {
            case State.Windup:
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                if (Time.time >= stateEndTime)
                {
                    FireAttack();
                }
                break;
            case State.Recover:
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                if (Time.time >= stateEndTime)
                {
                    state = State.Chase;
                }
                break;
            default:
                ChaseAndChooseAttack();
                break;
        }
    }

    private void ChaseAndChooseAttack()
    {
        float distance = Mathf.Abs(player.position.x - transform.position.x);
        SetFacing(player.position.x >= transform.position.x ? 1 : -1);

        if (Time.time >= nextCircleAttackTime && distance <= circleRange)
        {
            queuedAttack = AttackType.Circle;
            state = State.Windup;
            stateEndTime = Time.time + windupDuration;
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            HitFlash2D.Play(gameObject, new Color(0.45f, 1f, 0.55f), windupDuration);
            return;
        }

        if (Time.time >= nextAttackTime && distance <= triangleRange)
        {
            queuedAttack = distance <= knifeRange ? AttackType.Knife : AttackType.Triangle;
            state = State.Windup;
            stateEndTime = Time.time + windupDuration;
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            HitFlash2D.Play(gameObject, queuedAttack == AttackType.Knife ? Color.white : new Color(1f, 0.65f, 0.1f), windupDuration);
            return;
        }

        body.linearVelocity = new Vector2(facingDirection * moveSpeed, body.linearVelocity.y);
    }

    private void FireAttack()
    {
        state = State.Attack;

        if (queuedAttack == AttackType.Knife)
        {
            SpawnMeleeHitbox("Heavy Knife Hitbox", new Vector2(facingDirection * 1.6f, 0f), new Vector2(2f, 2.4f), knifeDamage, Color.white);
        }
        else if (queuedAttack == AttackType.Circle)
        {
            SpawnMeleeHitbox("Heavy Circle Hitbox", Vector2.zero, new Vector2(circleRange, circleRange), circleDamage, new Color(0.45f, 1f, 0.55f));
            nextCircleAttackTime = Time.time + circleAttackCooldown;
        }
        else
        {
            SpawnMeleeHitbox("Heavy Triangle Hitbox", new Vector2(facingDirection * 2.3f, 0f), new Vector2(3.5f, 2.6f), triangleDamage, new Color(1f, 0.65f, 0.1f));
        }

        Invoke(nameof(EnterRecovery), hitboxDuration);
    }

    private void SpawnMeleeHitbox(string name, Vector2 offset, Vector2 size, float damage, Color color)
    {
        GameObject hitbox = new GameObject(name);
        hitbox.transform.position = transform.position + (Vector3)offset;
        hitbox.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = hitbox.AddComponent<SpriteRenderer>();
        renderer.sprite = GetAttackSprite();
        renderer.color = color;
        renderer.sortingOrder = 19;

        BoxCollider2D collider = hitbox.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = Vector2.one;

        Rigidbody2D hitboxBody = hitbox.AddComponent<Rigidbody2D>();
        hitboxBody.bodyType = RigidbodyType2D.Kinematic;
        hitboxBody.gravityScale = 0f;

        EnemyMeleeHitbox2D damageHitbox = hitbox.AddComponent<EnemyMeleeHitbox2D>();
        damageHitbox.Configure(damage, hitboxDuration);
    }

    private Sprite GetAttackSprite()
    {
        if (queuedAttack == AttackType.Triangle)
        {
            return ShapeSprites.Triangle;
        }

        if (queuedAttack == AttackType.Circle)
        {
            return ShapeSprites.Circle;
        }

        return ShapeSprites.Knife;
    }

    private void EnterRecovery()
    {
        nextAttackTime = Time.time + attackCooldown;
        state = State.Recover;
        stateEndTime = Time.time + recoveryDuration;
    }

    private void SetFacing(int direction)
    {
        facingDirection = direction >= 0 ? 1 : -1;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facingDirection;
        transform.localScale = scale;
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }
}
