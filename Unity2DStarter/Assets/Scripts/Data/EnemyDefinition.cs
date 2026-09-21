using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Tales of Ivory Moss/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    public string displayName = "Enemy";
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float contactDamage = 10f;
    public float attackDamage = 10f;
    public float attackRange = 6f;
    public float attackCooldown = 1.5f;
    public GameObject enemyPrefab;
    public GameObject projectilePrefab;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
}
