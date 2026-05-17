using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FlyingEnemyAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 17.5f;
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float homeLeashRadius = 8f;
    [SerializeField] private float orbitRadius = 2.2f;
    [SerializeField] private float dashDamage = 10f;
    [SerializeField] private float dashCooldown = 2.2f;
    [SerializeField] private float windupDuration = 0.45f;
    [SerializeField] private float dashDuration = 0.35f;
    [SerializeField] private float recoveryDuration = 0.9f;

    private Rigidbody2D body;
    private Transform player;
    private Vector3 homePosition;
    private Vector2 dashDirection;
    private float nextAttackTime;
    private float stateEndTime;
    private float orbitAngle;
    private bool dealtDamageThisDash;
    private State state;

    private enum State
    {
        Patrol,
        Orbit,
        Windup,
        Dash,
        Recover
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        Collider2D bodyCollider = GetComponent<Collider2D>();
        bodyCollider.isTrigger = true;

        homePosition = transform.position;
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
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
        }

        switch (state)
        {
            case State.Windup:
                UpdateWindup();
                break;
            case State.Dash:
                UpdateDash();
                break;
            case State.Recover:
                UpdateRecover();
                break;
            default:
                UpdateMoveState();
                break;
        }
    }

    private void UpdateMoveState()
    {
        if (player != null && Vector2.Distance(player.position, homePosition) <= homeLeashRadius)
        {
            state = State.Orbit;
            orbitAngle += Time.deltaTime * moveSpeed * 0.7f;
            Vector2 target = (Vector2)player.position + new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
            MoveToward(target, moveSpeed);

            if (Time.time >= nextAttackTime)
            {
                state = State.Windup;
                stateEndTime = Time.time + windupDuration;
                dashDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
                body.linearVelocity = Vector2.zero;
                HitFlash2D.Play(gameObject, new Color(1f, 0.85f, 0.15f), windupDuration);
            }
        }
        else
        {
            state = State.Patrol;
            float patrolAngle = Time.time * 1.2f;
            Vector2 target = (Vector2)homePosition + new Vector2(Mathf.Cos(patrolAngle), Mathf.Sin(patrolAngle * 0.7f)) * patrolRadius;
            MoveToward(target, moveSpeed * 0.45f);
        }
    }

    private void UpdateWindup()
    {
        body.linearVelocity = Vector2.zero;
        if (Time.time < stateEndTime)
        {
            return;
        }

        state = State.Dash;
        dealtDamageThisDash = false;
        stateEndTime = Time.time + dashDuration;
        body.linearVelocity = dashDirection * moveSpeed;
    }

    private void UpdateDash()
    {
        if (Time.time >= stateEndTime)
        {
            EnterRecover();
        }
    }

    private void UpdateRecover()
    {
        body.linearVelocity = Vector2.zero;
        if (Time.time >= stateEndTime)
        {
            state = State.Patrol;
        }
    }

    private void EnterRecover()
    {
        body.linearVelocity = Vector2.zero;
        state = State.Recover;
        stateEndTime = Time.time + recoveryDuration;
        nextAttackTime = Time.time + dashCooldown;
    }

    private void MoveToward(Vector2 target, float speed)
    {
        Vector2 direction = target - (Vector2)transform.position;
        body.linearVelocity = direction.sqrMagnitude > 0.05f ? direction.normalized * speed : Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state != State.Dash || dealtDamageThisDash)
        {
            return;
        }

        if (EnemyDamage2D.TryDamagePlayer(other, dashDamage))
        {
            dealtDamageThisDash = true;
            EnterRecover();
        }
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
