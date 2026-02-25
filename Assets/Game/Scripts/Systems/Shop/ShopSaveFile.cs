using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShopSaveFile
{
    private const string FILE_NAME = "shopitems.json";

    private string FilePath =>
        Path.Combine(Application.persistentDataPath, FILE_NAME); //đường dẫn file

    public List<ShopItemProgress> Load()
    {
        if (!File.Exists(FilePath))
            return null;

        string json = File.ReadAllText(FilePath);
        var wrapper = JsonUtility.FromJson<ShopSaveWrapper>(json);

        return wrapper?.items;
    }

    public void Save(List<ShopItemProgress> items)
    {
        var wrapper = new ShopSaveWrapper
        {
            items = items
        };

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(FilePath, json);
    }
}
