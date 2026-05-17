using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    public const int SlotCount = 5;

    public static bool HasSave(int slot)
    {
        return File.Exists(GetSlotPath(slot));
    }

    public static bool HasAnySave()
    {
        for (int slot = 1; slot <= SlotCount; slot++)
        {
            if (HasSave(slot))
            {
                return true;
            }
        }

        return false;
    }

    public static SaveData ReadSlot(int slot)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void WriteSlot(int slot)
    {
        SaveData data = CaptureCurrentState();
        Directory.CreateDirectory(GetSaveDirectory());
        File.WriteAllText(GetSlotPath(slot), JsonUtility.ToJson(data, true));
    }

    public static void DeleteSlot(int slot)
    {
        string path = GetSlotPath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static string GetSlotSummary(int slot)
    {
        SaveData data = ReadSlot(slot);
        if (data == null)
        {
            return "Slot " + slot + " - Empty";
        }

        string scene = string.IsNullOrEmpty(data.sceneName) ? "Unknown" : data.sceneName;
        return "Day " + data.day + " - " + scene + " - " + data.difficulty;
    }

    private static SaveData CaptureCurrentState()
    {
        GameObject player = SystemsBootstrap.Instance != null ? SystemsBootstrap.Instance.Player : GameObject.Find("Player");
        PlayerStats stats = player != null ? player.GetComponent<PlayerStats>() : null;
        PlayerActionCounter actionCounter = player != null ? player.GetComponent<PlayerActionCounter>() : null;
        Vector3 position = player != null ? player.transform.position : Vector3.zero;

        SaveData data = new SaveData
        {
            savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            sceneName = SceneManager.GetActiveScene().name,
            difficulty = GameSession.CurrentDifficulty,
            day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1,
            playerX = position.x,
            playerY = position.y,
            playerZ = position.z,
            health = stats != null ? stats.Health : 100f,
            mana = stats != null ? stats.Mana : 100f,
            actionCounts = SessionStats.ToSaveList(),
            defeatedEnemyIds = GameSession.GetDefeatedEnemyIds()
        };

        if (actionCounter != null)
        {
            foreach (ActionCountSave count in actionCounter.ToSaveList())
            {
                UpsertCount(data, count.name, count.count);
            }
        }

        return data;
    }

    private static void UpsertCount(SaveData data, string name, int count)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        for (int i = 0; i < data.actionCounts.Count; i++)
        {
            if (data.actionCounts[i].name == name)
            {
                data.actionCounts[i].count = Mathf.Max(data.actionCounts[i].count, count);
                return;
            }
        }

        data.actionCounts.Add(new ActionCountSave
        {
            name = name,
            count = count
        });
    }

    private static string GetSaveDirectory()
    {
        return Path.Combine(Application.persistentDataPath, "Saves");
    }

    private static string GetSlotPath(int slot)
    {
        int clampedSlot = Mathf.Clamp(slot, 1, SlotCount);
        return Path.Combine(GetSaveDirectory(), "slot" + clampedSlot + ".json");
    }
}
