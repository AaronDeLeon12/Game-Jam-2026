/// <summary>
/// Global flag for the safe "home" state. While active: HUD hidden, no
/// attacking, no dashing, movement slowed. Ducking / walking / jumping stay.
/// Set by HomeScene while that scene is loaded.
/// </summary>
public static class HomeMode
{
    public static bool IsActive { get; set; }

    /// <summary>Movement speed multiplier applied while at home (-30%).</summary>
    public const float MoveSpeedMultiplier = 0.7f;
}
