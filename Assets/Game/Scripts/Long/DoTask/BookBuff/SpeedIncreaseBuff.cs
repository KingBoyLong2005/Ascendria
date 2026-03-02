using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Buffs/Speed Increase")]
public class SpeedIncreaseBuff : BookBuff
{
    public float amount = 10f;

    private struct UpgradeConfig
    {
        public int count;
        public float speed;
        public UpgradeConfig(int c, float s) { count = c; speed = s; }
    }

    private static readonly Dictionary<Rarity, UpgradeConfig> configs = new()
    {
        { Rarity.Common,    new(1, 5f)  },
        { Rarity.Uncommon,  new(1, 10f) },
        { Rarity.Rare,      new(1, 20f) },
        { Rarity.Epic,      new(1, 35f) },
        { Rarity.Legendary, new(1, 50f) },
    };

    public override void Apply()
    {
        PlayerStatManager.Instance.ModifyMoveSpeed(amount);
    }

    public override void LevelUp(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return;

        var stats = UpgradeHelper.GetRandomStats(cfg.count, 1, level);
        foreach (int stat in stats)
        {
            if (stat == 0) amount += cfg.speed;
        }

        level++;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return "Unknown";
        return $"+{cfg.speed} Move Speed";
    }

    public override List<string> GetUpgradePreview(Rarity rarity, int seed)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return new List<string> { "Unknown" };

        var stats = UpgradeHelper.GetRandomStats(cfg.count, 1, seed);
        var preview = new List<string>();
        foreach (int stat in stats)
            preview.Add($"+{cfg.speed} Move Speed");

        return preview;
    }
}