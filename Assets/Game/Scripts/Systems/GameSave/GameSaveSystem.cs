using System.Collections.Generic;
using UnityEngine;

public class GameSaveSystem
{
    private static GameSaveSystem instance;
    public static GameSaveSystem Instance
    {
        get
        {
            if (instance == null)
                instance = new GameSaveSystem();
            return instance;
        }
    }

    private GameSaveData saveData;
    private GameSaveFile saveFile;

    private GameSaveSystem()
    {
        saveFile = new GameSaveFile();
    }

    public void Initialize()
    {
        saveData = saveFile.Load();

        if (saveData == null)
        {
            Debug.Log("[GameSaveSystem] First run → create default save");

            saveData = CreateDefaultSave();
            Save();
        }
    }

    private GameSaveData CreateDefaultSave()
    {
        return new GameSaveData
        {
            currentCoin = 100000,
            highScores = new List<int>(),
            weapons = new List<UnlockEntry>
            {
                new UnlockEntry ("FireBall",true),
                new UnlockEntry ("Sword",true),
                new UnlockEntry ("Lightning Strike",false),
                new UnlockEntry ("Dice",false),
                new UnlockEntry ("Aura",false),
                new UnlockEntry ("Bullet Bounce",false),
                new UnlockEntry ("Buff Damage",true),
                new UnlockEntry ("Buff Speed",false),
                new UnlockEntry ("Buff Luck",false),
                new UnlockEntry ("Buff Health",true),
                new UnlockEntry ("Buff Cooldown",true),
            },
            characters = new List<UnlockEntry>
            {
                new UnlockEntry ("Fox Wizard",true),
                new UnlockEntry ("Knight",true),
                new UnlockEntry ("Cowboy Robot",false)
            }
        };
    }

    public void Save()
    {
        saveFile.Save(saveData);
    }

    // ================= COIN =================
    public int GetCoin() => saveData.currentCoin;

    public void AddCoin(int amount)
    {
        saveData.currentCoin += amount;
        Save();
    }

    public bool SpendCoin(int amount)
    {
        if (saveData.currentCoin < amount)
            return false;

        saveData.currentCoin -= amount;
        Save();
        return true;
    }

    // ================= HIGH SCORE =================
    public void AddScore(int score)
    {
        saveData.highScores.Add(score);
        saveData.highScores.Sort((a, b) => b.CompareTo(a));

        if (saveData.highScores.Count > 10)
            saveData.highScores.RemoveRange(10, saveData.highScores.Count - 10);

        Save();
    }

    public IReadOnlyList<int> GetHighScores() => saveData.highScores;

    // ================= ITEM =================
    public bool IsItemUnlocked(string id)
    {
        var entry = saveData.weapons.Find(x => x.id == id);
        return entry != null && entry.isUnlocked;
    }

    public void UnlockItem(string id)
    {
        var entry = saveData.weapons.Find(x => x.id == id);

        if (entry == null)
        {
            saveData.weapons.Add(new UnlockEntry(id,true));
        }
        else
        {
            entry.isUnlocked = true;
        }

        Save();
    }

    // ================= CHARACTER =================
    public bool IsCharacterUnlocked(string id)
    {
        var entry = saveData.characters.Find(x => x.id == id);
        return entry != null && entry.isUnlocked;
    }

    public void UnlockCharacter(string id)
    {
        var entry = saveData.characters.Find(x => x.id == id);

        if (entry == null)
        {
            saveData.characters.Add(new UnlockEntry(id,true));
        }
        else
        {
            if(entry.id == "Cowboy Robot")
            {
                UnlockItem("Bullet Bounce");
                entry.isUnlocked = true;
            }    
            else
            entry.isUnlocked = true;
        }

        Save();
    }
}
