using UnityEngine;

public static class EnemyDamage2D
{
    public static bool TryDamagePlayer(GameObject playerObject, float damage)
    {
        if (playerObject == null)
        {
            return false;
        }

        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            return false;
        }

        CircleShield shield = playerStats.GetComponentInChildren<CircleShield>();
        if (shield != null)
        {
            shield.TakeDamage(DifficultyRules.AdjustEnemyDamage(damage));
            return true;
        }

        playerStats.TakeDamage(DifficultyRules.AdjustEnemyDamage(damage));
        return true;
    }

    public static bool TryDamagePlayer(Collider2D other, float damage)
    {
        if (other == null)
        {
            return false;
        }

        return TryDamagePlayer(other.gameObject, damage);
    }
}
