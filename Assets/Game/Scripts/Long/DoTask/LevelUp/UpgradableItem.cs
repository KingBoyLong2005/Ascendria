using System.Collections.Generic;
using UnityEngine;

public abstract class UpgradableItem : ScriptableObject
{
    public Sprite Icon;
    public int level = 1;

    public abstract void LevelUp(Rarity rarity);

    // thêm hàm mô tả để UI hiển thị:
    public virtual string GetUpgradeDescription(Rarity rarity)
    {
        return "";
    }
    public virtual List<string> GetUpgradePreview(Rarity rarity, int seed)
    {
        return new List<string> { "No preview available" };
    }

}
// ==================== BASE HELPER ====================
public static class UpgradeHelper
{
    /// <summary>
    /// Random chọn stats với seed cố định
    /// </summary>
    public static List<int> GetRandomStats(int count, int maxStats, int seed)
    {
        UnityEngine.Random.InitState(seed);
        
        var available = new List<int>();
        for (int i = 0; i < maxStats; i++)
            available.Add(i);
        
        var selected = new List<int>();
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            selected.Add(available[idx]);
            available.RemoveAt(idx);
        }
        
        // Reset random state
        UnityEngine.Random.InitState(System.Environment.TickCount);
        
        return selected;
    }
    
    /// <summary>
    /// Format percentage
    /// </summary>
    public static string FormatPercent(float multiplier)
    {
        return $"+{(multiplier - 1f) * 100f:F0}%";
    }
}
