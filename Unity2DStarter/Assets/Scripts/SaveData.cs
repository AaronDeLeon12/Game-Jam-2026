using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 1;
    public string savedAt;
    public string sceneName;
    public GameDifficulty difficulty;
    public int day;
    public float playerX;
    public float playerY;
    public float playerZ;
    public float health;
    public float mana;
    public List<ActionCountSave> actionCounts = new List<ActionCountSave>();
}

[Serializable]
public class ActionCountSave
{
    public string name;
    public int count;
}
