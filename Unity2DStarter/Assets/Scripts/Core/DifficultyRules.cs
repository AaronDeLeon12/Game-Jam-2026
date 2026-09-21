using UnityEngine;
public static class DifficultyRules
{
    private static DifficultyDefinition current;
    private static DifficultyDefinition Definition
    {
        get { if (current == null || current.difficulty != GameSession.CurrentDifficulty) current = Resources.Load<DifficultyDefinition>("Definitions/" + GameSession.CurrentDifficulty); return current; }
    }
    public static float ManaRegenDelayMultiplier => Definition != null ? Definition.manaRegenDelayMultiplier : 1;
    public static float EnemyRangeMultiplier => Definition != null ? Definition.enemyRangeMultiplier : 1;
    public static float EnemyAggressionMultiplier => Definition != null ? Definition.enemyAggressionMultiplier : 1;
    public static float EnemyCooldownMultiplier => Definition != null ? Definition.enemyCooldownMultiplier : 1;
    public static float AdjustEnemyDamage(float damage) => Mathf.Max(1, damage + (Definition != null ? Definition.enemyDamageBonus : 0));
    public static bool DashHasManaCost => Definition != null && Definition.dashCostsMana;
}
