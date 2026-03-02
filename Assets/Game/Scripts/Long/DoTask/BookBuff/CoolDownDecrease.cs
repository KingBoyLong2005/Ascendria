using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Buffs/Cooldown Decrease")]
public class CoolDownDecrease : BookBuff
{
    [Tooltip("Percent, 0.05 = 5%")]
    public float amount = 0.05f;

    private struct UpgradeConfig
    {
        public int count;
        public float reduction;
        public UpgradeConfig(int c, float r) { count = c; reduction = r; }
    }

    private static readonly Dictionary<Rarity, UpgradeConfig> configs = new()
    {
        { Rarity.Common,    new(1, 0.08f) },  // 8%
        { Rarity.Uncommon,  new(1, 0.09f) },  // 9%
        { Rarity.Rare,      new(1, 0.11f) },  // 11%
        { Rarity.Epic,      new(1, 0.12f) },  // 12%
        { Rarity.Legendary, new(1, 0.15f) },  // 15%
    };

    public override void Apply()
    {
        foreach (var weapon in WeaponManager.Instance.weapons)
            weapon.ApplyCooldownReduction(amount); // amount vẫn là flat percent, giữ nguyên
    }

    public override void LevelUp(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return;
        var stats = UpgradeHelper.GetRandomStats(cfg.count, 1, level);
        foreach (int stat in stats)
            if (stat == 0) amount += cfg.reduction; // cộng dồn %
        level++;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return "Unknown";
        return $"-{cfg.reduction * 100f}% Weapon Cooldown";
    }

    public override List<string> GetUpgradePreview(Rarity rarity, int seed)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return new List<string> { "Unknown" };

        var stats = UpgradeHelper.GetRandomStats(cfg.count, 1, seed);
        var preview = new List<string>();
        foreach (int stat in stats)
            preview.Add($"-{cfg.reduction * 100f}% Weapon Cooldown");

        return preview;
    }
}