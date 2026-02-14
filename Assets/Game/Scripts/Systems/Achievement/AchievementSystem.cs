using UnityEngine;
using System.Collections.Generic;

public class AchievementSystem
{
    private static AchievementSystem instance;
    public static AchievementSystem Instance
    {
        get
        {
            if (instance == null)
                instance = new AchievementSystem();
            return instance;
        }
    }

    private AchievementDatabase database;
    private List<AchievementProgress> progressList;
    private AchievementSaveFile saveFile;

    private AchievementSystem()
    {
        saveFile = new AchievementSaveFile();
    }

    //Gọi khởi tạo AchievementSystem tại scene đầu tiên (có lẽ cần trước MenuScene)
    //sau này cần thêm loading scene để load các dữ liệu đã lưu local như game NinjaSurvival nữa

    public void Initialize()
    {
        database = Resources.Load<AchievementDatabase>("Achievement/AchievementDB");

        //Test log
        if (database == null)
        {
            Debug.LogError("❌ AchievementDB NOT FOUND in Resources folder!");
            return;
        }

        Debug.Log("✅ AchievementDB loaded successfully!");

        if (database.achievements == null || database.achievements.Length == 0)
        {
            Debug.LogWarning("⚠ AchievementDB loaded but achievements array is empty!");
        }
        else
        {
            Debug.Log($"📦 Total Achievements: {database.achievements.Length}");

            foreach (var achievement in database.achievements)
            {
                if (achievement == null)
                {
                    Debug.LogWarning("⚠ Found NULL achievement in database!");
                    continue;
                }

                Debug.Log(
                    $"ID: {achievement.id} | Name: {achievement.displayName} | Target: {achievement.target}"
                );
            }
        }
        //end test log

        progressList = saveFile.Load();

        if (progressList == null)
        {
            progressList = CreateInitialProgress();
        }

        SyncWithDatabase();
        saveFile.Save(progressList);
    }

    //Tạo mới
    private List<AchievementProgress> CreateInitialProgress()
    {
        var list = new List<AchievementProgress>();

        foreach (var def in database.achievements)
        {
            list.Add(new AchievementProgress
            {
                id = def.id,
                current = 0,
                target = def.target,
                isCompleted = false
            });
        }

        return list;
    }

    private void SyncWithDatabase()
    {
        foreach (var def in database.achievements)
        {
            bool exists = progressList.Exists(p => p.id == def.id);
            if (!exists) //tạo nếu chưa có
            {
                progressList.Add(new AchievementProgress
                {
                    id = def.id,
                    current = 0,
                    target = def.target,
                    isCompleted = false
                });
            }
        }
    }

    //Cập nhật tiến độ
    public void ApplyProgressChanges(Dictionary<string, int> changes)
    {
        //Cập nhật tiến độ sau ván đấu
        //Lưu tiến độ vào local
    }
}
