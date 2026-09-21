using UnityEngine;

[CreateAssetMenu(menuName = "Tales of Ivory Moss/PlayerCombatDefinition") ]
public class PlayerCombatDefinition : ScriptableObject
{
    [Min(0)] public float projectileSpeed = 13f;
    [Min(0)] public float projectileRange = 3.5f;
    [Min(0)] public float squareManaCost = 20f;
    [Min(0)] public float squareDamage = 33.4f;
    [Min(0)] public float squareCooldown = 0.8f;
    [Min(0)] public float triangleManaCost = 30f;
    [Min(0)] public float triangleDamage = 100f;
    [Min(0)] public float triangleCooldown = 2f;
    [Min(0)] public float triangleFullChargeTime = 0.8f;
    [Min(0)] public float triangleOverchargeTime = 4f;
    [Min(0)] public float triangleOverchargeSelfDamage = 20f;
    [Min(0)] public float triangleMinDamageAsSquarePercent = 0.55f;
    [Min(0)] public float triangleMinRangeAsSquarePercent = 0.75f;
    [Min(0)] public float triangleMaxRangeAsSquarePercent = 1.5f;
    [Min(0)] public float circleManaCost = 10f;
    [Min(0)] public float circleShieldHealth = 20f;
    [Min(0)] public float circleShieldDuration = 2f;
    [Min(0)] public float circleCooldown = 0.4f;
    [Min(0)] public float circleShieldScale = 4.2f;
    [Min(0)] public float knifeDamage = 10f;
    [Min(0)] public float knifeResourceValue = 20f;
    [Min(0)] public float knifeCooldown = 0.5f;
    [Min(0)] public float spellManaRegenDelay = 3f;
}
