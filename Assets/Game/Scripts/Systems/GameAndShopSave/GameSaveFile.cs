using System.IO;
using UnityEngine;

public class GameSaveFile
{
    private const string FILE_NAME = "gamesave.json";

    private string FilePath =>
        Path.Combine(Application.persistentDataPath, FILE_NAME);

    public GameSaveData Load()
    {
        if (!File.Exists(FilePath))
        {
            Debug.Log("[GameSave] No save file found.");
            return null;
        }

        string json = File.ReadAllText(FilePath);
        var wrapper = JsonUtility.FromJson<GameSaveWrapper>(json);

        Debug.Log("[GameSave] Loaded from: " + FilePath);
        return wrapper?.data;
    }

    public void Save(GameSaveData data)
    {
        var wrapper = new GameSaveWrapper
        {
            data = data
        };

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(FilePath, json);

        Debug.Log("[GameSave] Saved to: " + FilePath);
    }

}
