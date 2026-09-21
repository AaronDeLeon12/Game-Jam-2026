using UnityEngine;

[CreateAssetMenu(fileName = "SpellDefinition", menuName = "Tales of Ivory Moss/Spell Definition")]
public class SpellDefinition : ScriptableObject
{
    public SpellType spellType;
    public string displayName = "Spell";
    public float manaCost = 20f;
    public float cooldown = 1f;
    public float damage = 10f;
    public float reach = 3f;
    public float chargeTimeForFullPower;
    public float overchargeDamageTime = 4f;
    public Sprite hudIcon;
    public GameObject projectilePrefab;
    public GameObject effectPrefab;
    public AudioClip castSound;
}
