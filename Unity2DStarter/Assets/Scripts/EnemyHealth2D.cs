using UnityEngine;

public class EnemyHealth2D : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    private float health;

    public bool IsAlive => health > 0f;

    private void Awake()
    {
        if (GetComponent<EnemySaveTracker>() == null)
        {
            gameObject.AddComponent<EnemySaveTracker>();
        }

        health = maxHealth;
    }

    public void Configure(float newMaxHealth)
    {
        maxHealth = Mathf.Max(1f, newMaxHealth);
        health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (health <= 0f)
        {
            return;
        }

        health = Mathf.Max(0f, health - Mathf.Max(0f, amount));
        HitFlash2D.Play(gameObject, Color.white, 0.06f);
        GameAudio.PlaySfx("hitSFX", transform.position, 0.65f);

        if (health <= 0f)
        {
            SessionStats.Record("enemies_killed");
            EnemySaveTracker tracker = GetComponent<EnemySaveTracker>();
            if (tracker != null)
            {
                tracker.RecordDefeat();
            }

            Destroy(gameObject);
        }
    }
}
