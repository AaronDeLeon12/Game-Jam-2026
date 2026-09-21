using UnityEngine;

public static class GameModal
{
    public static bool IsOpen { get; private set; }

    private static float previousTimeScale = 1f;

    public static void Open()
    {
        if (!IsOpen)
        {
            previousTimeScale = Time.timeScale;
        }

        IsOpen = true;
        Time.timeScale = 0f;
    }

    public static void Close()
    {
        IsOpen = false;
        if (!PauseMenu.IsPaused)
        {
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        }
    }
}
