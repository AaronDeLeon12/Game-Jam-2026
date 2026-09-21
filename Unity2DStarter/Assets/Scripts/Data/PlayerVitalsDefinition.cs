using UnityEngine;

[CreateAssetMenu(menuName = "Tales of Ivory Moss/PlayerVitalsDefinition") ]
public class PlayerVitalsDefinition : ScriptableObject
{
    [Min(0)] public float maxHealth = 100f;
    [Min(0)] public float maxMana = 100f;
    [Min(0)] public float spellManaCost = 20f;
    [Min(0)] public float manaRegenDelay = 3f;
    [Min(0)] public float manaRegenPerSecond = 20f;
    [Min(0)] public float healthValueInMana = 2f;
    [Min(0)] public float parryWindowDuration = 0.2f;
    [Min(0)] public float parryCooldown = 1f;
    [Min(0)] public float parryHealthRestore = 10f;
}
