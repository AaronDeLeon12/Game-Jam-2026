using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyDefinition", menuName = "Tales of Ivory Moss/Difficulty Definition")]
public class DifficultyDefinition : ScriptableObject
{
    public GameDifficulty difficulty = GameDifficulty.Normal;
    public float playerDamageMultiplier = 1f;
    public float playerHealthMultiplier = 1f;
    public float manaRegenDelayMultiplier = 1f;
    public float enemyDamageBonus;
    public float enemyRangeMultiplier = 1f;
    public float enemyAggressionMultiplier = 1f;
    public float enemyCooldownMultiplier = 1f;
    public bool dashCostsMana;
}
