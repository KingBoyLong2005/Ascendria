using System.Collections.Generic;
using UnityEngine;

public class ShopSystem
{
    private static ShopSystem instance;
    public static ShopSystem Instance
    {
        get
        {
            if (instance == null)
                instance = new ShopSystem();
            return instance;
        }
    }

    private ShopDB shopDB;
    private ShopSaveFile saveFile;
    private List<ShopItemProgress> progressList;
    private Dictionary<string, ShopItemProgress> progressDict = new Dictionary<string, ShopItemProgress>();


    private ShopSystem()
    {
        saveFile = new ShopSaveFile();
    }

    public void Initialize()
    {
        shopDB = Resources.Load<ShopDB>("ShopItem/ShopDB");

        progressList = saveFile.Load();

        if (progressList == null || progressList.Count == 0)
        {
            Debug.Log("Create new shop save");
            progressList = CreateInitialProgress();
            saveFile.Save(progressList);
        }

        RebuildDictionary();
        SyncWithDatabase();

        saveFile.Save(progressList);
    }

    //CREATE NEW
    private List<ShopItemProgress> CreateInitialProgress()
    {
        var list = new List<ShopItemProgress>();

        foreach (var item in shopDB.items)
        {
            list.Add(new ShopItemProgress
            {
                id = item.id,
                currentLevel = 0
            });
        }

        return list;
    }

    //SYNC
    private void SyncWithDatabase()
    {
        bool changed = false;

        foreach (var def in shopDB.items)
        {
            if (!progressDict.ContainsKey(def.id))
            {
                var prog = new ShopItemProgress
                {
                    id = def.id,
                    currentLevel = 0
                };

                progressList.Add(prog);
                progressDict.Add(def.id, prog);
                changed = true;
            }
        }

        if (changed)
            Debug.Log("Shop progress synced with database");
    }

    private void RebuildDictionary()
    {
        progressDict.Clear();

        foreach (var p in progressList)
        {
            progressDict[p.id] = p;
        }
    }

    //QUERY
    public int GetCurrentLevel(string id)
    {
        if (progressDict.TryGetValue(id, out var prog))
            return prog.currentLevel;

        return 0;
    }

    public float GetCurrentValue(string id)
    {
        var def = shopDB.GetItem(id);
        if (def == null) return 0;

        int level = GetCurrentLevel(id);
        if (level <= 0) return 0;

        var data = def.GetLevelData(level);
        return data != null ? data.value : 0;
    }

    public int GetUpgradeCost(string id)
    {
        var def = shopDB.GetItem(id);
        if (def == null) return -1;

        int nextLevel = GetCurrentLevel(id) + 1;
        if (nextLevel > def.maxLevel)
            return -1;

        return def.GetLevelData(nextLevel).upgradeCost;
    }

    public bool IsMaxLevel(string id)
    {
        var def = shopDB.GetItem(id);
        if (def == null) return true;

        return GetCurrentLevel(id) >= def.maxLevel;
    }

    //LEVEL UP
    public bool TryLevelUp(string id)
    {
        if (IsMaxLevel(id))
            return false;

        int cost = GetUpgradeCost(id);

        if(GameSaveSystem.Instance.SpendCoin(cost))
        {
            progressDict[id].currentLevel++;

            saveFile.Save(progressList);
            return true;
        }    
        else
            return false;
    }
    public ShopItemDefinition GetDefinition(string id) => shopDB?.GetItem(id);
}
