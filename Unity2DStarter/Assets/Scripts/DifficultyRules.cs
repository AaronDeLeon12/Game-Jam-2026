using UnityEngine;

public static class DifficultyRules
{
    public static float ManaRegenDelayMultiplier
    {
        get
        {
            return GameSession.CurrentDifficulty == GameDifficulty.Easy ? 0.5f : 1f;
        }
    }

    public static float EnemyRangeMultiplier
    {
        get
        {
            switch (GameSession.CurrentDifficulty)
            {
                case GameDifficulty.Easy:
                    return 0.75f;
                case GameDifficulty.Hard:
                    return 1.25f;
                default:
                    return 1f;
            }
        }
    }

    public static float EnemyAggressionMultiplier
    {
        get
        {
            return GameSession.CurrentDifficulty == GameDifficulty.Hard ? 1.15f : 1f;
        }
    }

    public static float EnemyCooldownMultiplier
    {
        get
        {
            return GameSession.CurrentDifficulty == GameDifficulty.Hard ? 0.8f : 1f;
        }
    }

    public static float AdjustEnemyDamage(float damage)
    {
        switch (GameSession.CurrentDifficulty)
        {
            case GameDifficulty.Easy:
                return Mathf.Max(1f, damage - 5f);
            case GameDifficulty.Hard:
                return damage + 5f;
            default:
                return damage;
        }
    }

    public static bool DashHasManaCost => GameSession.CurrentDifficulty == GameDifficulty.Hard;
}
