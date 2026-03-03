using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Buffs/Speed Increase")]
public class SpeedIncreaseBuff : BookBuff
{
    public float amount = 1f;

private struct UpgradeConfig
{
    public int count;
    public float mult;
    public UpgradeConfig(int c, float m) { count = c; mult = m; }
    }

    private static readonly Dictionary<Rarity, UpgradeConfig> configs = new()
    {
        { Rarity.Common,    new(1, 0.08f) },
        { Rarity.Uncommon,  new(1, 0.10f) },
        { Rarity.Rare,      new(1, 0.11f) },
        { Rarity.Epic,      new(1, 0.13f) },
        { Rarity.Legendary, new(1, 0.16f) },
    };

    public override void Apply()
    {
        PlayerStatManager.Instance.ModifyMoveSpeed(0f, amount);
    }

    public override void LevelUp(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return;
        var stats = UpgradeHelper.GetRandomStats(cfg.count, 1, level);
        foreach (int stat in stats)
            if (stat == 0) amount += cfg.mult;
        level++;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return "Unknown";
        return $"+{cfg.mult * 100f}% Move Speed";
    }

    public override List<string> GetUpgradePreview(Rarity rarity, int seed)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return new List<string> { "Unknown" };
        var stats = UpgradeHelper.GetRandomStats(cfg.count, 1, seed);
        var preview = new List<string>();
        foreach (int stat in stats)
            preview.Add($"+{cfg.mult * 100f}% Move Speed");
        return preview;
    }
}