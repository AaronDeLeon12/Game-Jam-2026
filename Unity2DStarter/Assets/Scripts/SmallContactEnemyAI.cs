using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SmallContactEnemyAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10.5f;
    [SerializeField] private float contactDamage = 15f;
    [SerializeField] private float contactCooldown = 1f;
    [SerializeField] private float recoveryDuration = 0.45f;

    private Rigidbody2D body;
    private Transform player;
    private float nextDamageTime;
    private float recoveryEndTime;

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
        body.linearVelocity = new Vector2(direction * moveSpeed, body.linearVelocity.y);
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

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }
}
