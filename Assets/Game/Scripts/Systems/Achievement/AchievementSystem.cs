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

    public void Initialize()
    {
        database = Resources.Load<AchievementDatabase>("Achievement/AchievementDB");

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
        bool hasChanged = false;

        foreach (var change in changes)
        {
            var progress = progressList.Find(p => p.id == change.Key);
            if (progress == null)
                continue;

            if (progress.isCompleted)
                continue;

            int before = progress.current;

            progress.current += change.Value;

            var def = GetDefinition(progress.id);
            if (progress.current >= def.target)
            {
                progress.current = def.target;
                progress.isCompleted = true;
            }

            if (before != progress.current)
                hasChanged = true;
        }

        if (hasChanged)
            saveFile.Save(progressList);
    }

    private AchievementDefinition GetDefinition(string id)
    {
        foreach (var def in database.achievements)
        {
            if (def.id == id)
                return def;
        }

        return null;
    }

    //Đọc tiến độ
    public void LogSavedProgress()
    {
        if (database == null)
        {
            Debug.LogWarning("AchievementDatabase is not assigned.");
            return;
        }

        if (progressList == null || progressList.Count == 0)
        {
            Debug.Log("No achievement progress found.");
            return;
        }

        Debug.Log("========== ACHIEVEMENT STATUS ==========");

        foreach (var def in database.achievements)
        {
            var progress = progressList.Find(p => p.id == def.id);

            int current = progress != null ? progress.current : 0;
            bool completed = progress != null && progress.isCompleted;

            Debug.Log(
                $"ID: {def.id}\n" +
                $"Display Name: {def.displayName}\n" +
                $"Description: {def.description}\n" +
                $"Progress: {current}/{def.target}\n" +
                $"Completed: {completed}\n" +
                $"---------------------------------------"
            );
        }

        Debug.Log("========================================");
    }
    /// <summary>
    /// Claim phần thưởng achievement. Trả về true nếu claim thành công.
    /// </summary>
    public bool ClaimAchievement(string id)
    {
        var progress = progressList.Find(p => p.id == id);

        if (progress == null)       return false;
        if (!progress.isCompleted)  return false;   // chưa hoàn thành
        if (progress.isClaimed)     return false;   // đã nhận rồi

        progress.isClaimed = true;
        saveFile.Save(progressList);
        return true;
    }
    public AchievementProgress GetProgress(string id)
    {
        return progressList?.Find(p => p.id == id);
    }

    public List<AchievementProgress> GetAllProgress()
    {
        return progressList;
    }

    public AchievementDefinition GetDefinitionPublic(string id)
    {
        return GetDefinition(id);
    }
}
