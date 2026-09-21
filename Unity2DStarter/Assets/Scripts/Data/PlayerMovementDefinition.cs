using UnityEngine;

[CreateAssetMenu(menuName = "Tales of Ivory Moss/PlayerMovementDefinition") ]
public class PlayerMovementDefinition : ScriptableObject
{
    [Min(0)] public float moveSpeed = 7f;
    [Min(0)] public float jumpHeight = 2.5f;
    [Min(0)] public float gravityScale = 5f;
    [Min(0)] public float fallGravityMultiplier = 1.7f;
    [Min(0)] public float glideGravityScale = 1.25f;
    [Min(0)] public float dashDistance = 3f;
    [Min(0)] public float dashDisappearTime = 0.4f;
    [Min(0)] public float dashCooldown = 1.5f;
    [Min(0)] public float duckSpeedMultiplier = 0.5f;
    [Min(0)] public float duckHitboxHeightMultiplier = 0.5f;
    [Min(0)] public float duckIdleVisualHeightMultiplier = 0.68f;
    [Min(0)] public float duckIdleVisualLowerAmount = 0.31f;
    [Min(0)] public float duckMoveVisualHeightMultiplier = 0.96f;
    [Min(0)] public float duckMoveVisualLowerAmount = 0.38f;
    [Min(0)] public float platformDropDuration = 0.35f;
}
