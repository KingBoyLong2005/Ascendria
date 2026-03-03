using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Buffs/Luck Increase")]
public class LuckBuff : BookBuff
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
        { Rarity.Common,    new(1, 0.07f) },
        { Rarity.Uncommon,  new(1, 0.08f) },
        { Rarity.Rare,      new(1, 0.10f) },
        { Rarity.Epic,      new(1, 0.11f) },
        { Rarity.Legendary, new(1, 0.14f) },
    };

    public override void Apply()
    {
        PlayerStatManager.Instance.ModifyLuck(0f, amount);
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
        return $"+{cfg.mult * 100f}% Luck";
    }

    public override List<string> GetUpgradePreview(Rarity rarity, int seed)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return new List<string> { "Unknown" };
        var stats = UpgradeHelper.GetRandomStats(cfg.count, 1, seed);
        var preview = new List<string>();
        foreach (int stat in stats)
            preview.Add($"+{cfg.mult * 100f}% Luck");
        return preview;
    }
}