using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack2D : MonoBehaviour
{
    private readonly HashSet<EnemyDummy> hitEnemies = new HashSet<EnemyDummy>();
    private GameObject owner;
    private float damage = 10f;
    private float resourceValue = 20f;
    private float expireTime;

    public void Launch(GameObject attackOwner, float attackDamage, float rewardResourceValue, float lifetime)
    {
        owner = attackOwner;
        damage = attackDamage;
        resourceValue = rewardResourceValue;
        expireTime = Time.time + lifetime;
    }

    private void Update()
    {
        if (Time.time >= expireTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyDummy enemy = other.GetComponent<EnemyDummy>();
        if (enemy == null || hitEnemies.Contains(enemy))
        {
            return;
        }

        hitEnemies.Add(enemy);
        enemy.TakeDamage(damage);
        GameAudio.PlaySfx("knifeHit", transform.position, 0.9f);

        PlayerStats playerStats = owner != null ? owner.GetComponent<PlayerStats>() : null;
        if (playerStats != null)
        {
            playerStats.RestoreResourceValue(resourceValue);
        }
    }
}
