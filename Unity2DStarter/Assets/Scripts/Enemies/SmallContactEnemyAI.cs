using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SmallContactEnemyAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.6f;
    [SerializeField] private float driftAcceleration = 10f;
    [SerializeField] private float contactDamage = 15f;
    [SerializeField] private float contactCooldown = 1f;
    [SerializeField] private float recoveryDuration = 0.45f;
    [SerializeField] private float dashRollInterval = 4f;
    [SerializeField] private float dashChance = 0.25f;
    [SerializeField] private float dashSpeedMultiplier = 3f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float duplicateRollInterval = 10f;
    [SerializeField] private float duplicateChance = 0.1f;
    [SerializeField] private int maxCloneGeneration = 2;

    private Rigidbody2D body;
    private Transform player;
    private float nextDamageTime;
    private float recoveryEndTime;
    private float nextDashRollTime;
    private float dashEndTime;
    private float nextDuplicateRollTime;
    private int cloneGeneration;
    private bool isDashing;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 5f;
        body.freezeRotation = true;
        FindPlayer();
        nextDashRollTime = Time.time + dashRollInterval;
        nextDuplicateRollTime = Time.time + duplicateRollInterval;
    }

    private void Update()
    {
        if (PauseMenu.IsPaused)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        if (Time.time < recoveryEndTime)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            return;
        }

        if (player == null)
        {
            FindPlayer();
            return;
        }

        float direction = Mathf.Sign(player.position.x - transform.position.x);
        HandleSpecialRolls(direction);

        if (isDashing)
        {
            if (Time.time >= dashEndTime)
            {
                isDashing = false;
            }
            else
            {
                body.linearVelocity = new Vector2(
                    direction * moveSpeed * dashSpeedMultiplier * DifficultyRules.EnemyAggressionMultiplier,
                    body.linearVelocity.y);
                return;
            }
        }

        float targetVelocityX = direction * moveSpeed * DifficultyRules.EnemyAggressionMultiplier;
        float velocityX = Mathf.MoveTowards(
            body.linearVelocity.x,
            targetVelocityX,
            driftAcceleration * Time.deltaTime);
        body.linearVelocity = new Vector2(velocityX, body.linearVelocity.y);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        if (EnemyDamage2D.TryDamagePlayer(collision.gameObject, contactDamage))
        {
            nextDamageTime = Time.time + contactCooldown;
            recoveryEndTime = Time.time + recoveryDuration;
        }
    }

    private void HandleSpecialRolls(float direction)
    {
        if (Time.time >= nextDashRollTime)
        {
            nextDashRollTime = Time.time + dashRollInterval;
            if (Random.value <= dashChance)
            {
                isDashing = true;
                dashEndTime = Time.time + dashDuration;
                HitFlash2D.Play(gameObject, new Color(1f, 0.85f, 0.15f), dashDuration);
            }
        }

        if (Time.time >= nextDuplicateRollTime)
        {
            nextDuplicateRollTime = Time.time + duplicateRollInterval;
            if (cloneGeneration < maxCloneGeneration && Random.value <= duplicateChance)
            {
                CreateClone(direction);
            }
        }
    }

    private void CreateClone(float direction)
    {
        GameObject clone = Instantiate(gameObject, transform.position + new Vector3(-direction * 0.85f, 0.35f, 0f), Quaternion.identity);
        clone.name = gameObject.name + " Clone";

        Rigidbody2D cloneBody = clone.GetComponent<Rigidbody2D>();
        if (cloneBody != null)
        {
            cloneBody.linearVelocity = Vector2.zero;
        }

        SmallContactEnemyAI cloneAi = clone.GetComponent<SmallContactEnemyAI>();
        if (cloneAi != null)
        {
            cloneAi.cloneGeneration = cloneGeneration + 1;
            cloneAi.ResetTimers();
        }

        EnemyHealth2D health = clone.GetComponent<EnemyHealth2D>();
        if (health != null)
        {
            health.Configure(80f);
        }
    }

    private void ResetTimers()
    {
        isDashing = false;
        recoveryEndTime = 0f;
        nextDashRollTime = Time.time + dashRollInterval;
        nextDuplicateRollTime = Time.time + duplicateRollInterval;
        FindPlayer();
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
