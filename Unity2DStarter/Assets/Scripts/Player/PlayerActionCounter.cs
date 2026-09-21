using System.Collections.Generic;
using UnityEngine;

public class PlayerActionCounter : MonoBehaviour
{
    private readonly Dictionary<string, int> counts = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> Counts => counts;

    public void Record(string actionName)
    {
        if (string.IsNullOrEmpty(actionName))
        {
            return;
        }

        counts.TryGetValue(actionName, out int count);
        counts[actionName] = count + 1;
        SessionStats.Record(actionName);
    }

    public int GetCount(string actionName)
    {
        return counts.TryGetValue(actionName, out int count) ? count : 0;
    }

    public System.Collections.Generic.List<ActionCountSave> ToSaveList()
    {
        System.Collections.Generic.List<ActionCountSave> saveCounts = new System.Collections.Generic.List<ActionCountSave>();
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

    public void LoadCounts(System.Collections.Generic.List<ActionCountSave> saveCounts)
    {
        counts.Clear();
        if (saveCounts == null)
        {
            return;
        }

        for (int i = 0; i < saveCounts.Count; i++)
        {
            ActionCountSave saveCount = saveCounts[i];
            if (saveCount != null && !string.IsNullOrEmpty(saveCount.name))
            {
                counts[saveCount.name] = saveCount.count;
            }
        }
    }
}
