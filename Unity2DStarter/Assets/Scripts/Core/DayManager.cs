using System;
using UnityEngine;

/// <summary>
/// Tracks the current in-game day as persistent state (the "long turn").
/// Independent singleton that survives scene loads and app restarts via
/// PlayerPrefs, so the home / world can change a little each day.
///
/// Lifecycle:
///  - Any scene can guarantee it exists with DayManager.EnsureExists().
///  - The main menu's "Play" should call StartNewGame() (resets to day 1).
///  - Advance the day with AdvanceDay() when a day ends.
///  - React to changes via the DayChanged event or by reading CurrentDay.
/// </summary>
public class DayManager : MonoBehaviour
{
    private const string DayKey = "CurrentDay";

    public static DayManager Instance { get; private set; }

    /// <summary>Raised whenever the day changes (advance, reset, set).</summary>
    public static event Action<int> DayChanged;

    [SerializeField] private int currentDay = 1;

    public int CurrentDay => currentDay;

    public static void EnsureExists()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject go = new GameObject("DayManager");
        go.AddComponent<DayManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentDay = Mathf.Max(1, PlayerPrefs.GetInt(DayKey, 1));
    }

    public void StartNewGame()
    {
        SetDay(1);
    }

    public void AdvanceDay()
    {
        SetDay(currentDay + 1);
    }

    public void SetDay(int day)
    {
        currentDay = Mathf.Max(1, day);
        PlayerPrefs.SetInt(DayKey, currentDay);
        PlayerPrefs.Save();
        DayChanged?.Invoke(currentDay);
    }
}
