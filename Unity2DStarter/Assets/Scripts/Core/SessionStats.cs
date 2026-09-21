using System.Collections.Generic;

public static class SessionStats
{
    private static readonly Dictionary<string, int> counts = new Dictionary<string, int>();

    public static IReadOnlyDictionary<string, int> Counts => counts;

    public static void ResetRun()
    {
        counts.Clear();
    }

    public static void Record(string statName)
    {
        if (string.IsNullOrEmpty(statName))
        {
            return;
        }

        counts.TryGetValue(statName, out int count);
        counts[statName] = count + 1;
    }

    public static int GetCount(string statName)
    {
        return counts.TryGetValue(statName, out int count) ? count : 0;
    }

    public static List<ActionCountSave> ToSaveList()
    {
        List<ActionCountSave> saveCounts = new List<ActionCountSave>();
        foreach (KeyValuePair<string, int> count in counts)
        {
            saveCounts.Add(new ActionCountSave
            {
                name = count.Key,
                count = count.Value
            });
        }

        return saveCounts;
    }

    public static void LoadFromSave(SaveData save)
    {
        counts.Clear();
        if (save == null || save.actionCounts == null)
        {
            return;
        }

        for (int i = 0; i < save.actionCounts.Count; i++)
        {
            ActionCountSave actionCount = save.actionCounts[i];
            if (actionCount != null && !string.IsNullOrEmpty(actionCount.name))
            {
                counts[actionCount.name] = actionCount.count;
            }
        }
    }
}
