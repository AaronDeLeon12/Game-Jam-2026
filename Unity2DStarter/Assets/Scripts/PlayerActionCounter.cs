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
    }

    public int GetCount(string actionName)
    {
        return counts.TryGetValue(actionName, out int count) ? count : 0;
    }
}
