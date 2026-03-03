using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Buffs/Health Increase")]
public class HealthIncreaseBuff : BookBuff
{
    public float amount = 10f;

    private struct UpgradeConfig
    {
        public int count;
        public float hp;
        public UpgradeConfig(int c, float h) { count = c; hp = h; }
    }

    private static readonly Dictionary<Rarity, UpgradeConfig> configs = new()
    {
        { Rarity.Common,    new(1, 5f)  },
        { Rarity.Uncommon,  new(1, 8f)  },
        { Rarity.Rare,      new(1, 10f) },
        { Rarity.Epic,      new(1, 13f) }, // ← đổi 20 → 13
        { Rarity.Legendary, new(1, 15f) }, // ← đổi 25 → 15
    };

    public override void Apply()
    {
        PlayerStatManager.Instance.ModifyHealth(amount);
    }

    public override void LevelUp(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return;

        var stats = UpgradeHelper.GetRandomStats(cfg.count, 1, level);
        foreach (int stat in stats)
        {
            if (stat == 0) amount += cfg.hp;
        }

        level++;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return "Unknown";
        return $"+{cfg.hp} Max HP";
    }

    public override List<string> GetUpgradePreview(Rarity rarity, int seed)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return new List<string> { "Unknown" };

        var stats = UpgradeHelper.GetRandomStats(cfg.count, 1, seed);
        var preview = new List<string>();
        foreach (int stat in stats)
            preview.Add($"+{cfg.hp} Max HP");

        return preview;
    }
}