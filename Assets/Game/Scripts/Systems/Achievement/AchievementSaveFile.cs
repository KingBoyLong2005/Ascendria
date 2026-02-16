using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AchievementSaveFile
{
    private const string FILE_NAME = "achievements.json";

    private string FilePath =>
        Path.Combine(Application.persistentDataPath, FILE_NAME); //đường dẫn file

    public List<AchievementProgress> Load()
    {
        if (!File.Exists(FilePath))
            return null;

        string json = File.ReadAllText(FilePath);
        var wrapper = JsonUtility.FromJson<AchievementSaveWrapper>(json);
        return wrapper?.achievements;
    }

    public void Save(List<AchievementProgress> achievements)
    {
        var wrapper = new AchievementSaveWrapper
        {
            achievements = achievements
        };

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(FilePath, json);
    }
}
