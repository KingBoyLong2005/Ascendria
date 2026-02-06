using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Weapons/Aura")]
public class AuraWeapon : Weapon
{
    public float auraDuration = 10f;
    public GameObject auraPrefab;
    public LayerMask enemyMask;


    private struct UpgradeConfig
    {
        public int count;
        public float dmgMultiplier, sizeMultiplier;
        public UpgradeConfig(int c, float dmg, float size)
        {
            count = c; dmgMultiplier = dmg; sizeMultiplier = size;
        }
    }

    private static readonly Dictionary<Rarity, UpgradeConfig> configs = new()
    {
        { Rarity.Common,     new(1, 1.014f, 2.14f) },
        { Rarity.Uncommon,   new(1, 1.017f, 1.17f) },
        { Rarity.Rare,       new(2, 1.02f, 1.20f) },
        { Rarity.Epic,       new(2, 1.022f, 1.22f) },
        { Rarity.Legendary,  new(2, 1.028f, 1.28f) }
    };

    public override void Attack(WeaponContext ctx)
    {
        GameObject go = PoolManager.Spawn(auraPrefab, ctx.owner.position, auraPrefab.transform.rotation);
        
        if (go != null)
        {
            go.transform.SetParent(ctx.owner);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one * baseSize;
            go.GetComponent<AuraEffect>()?.Initialize(baseDamage, baseRange, auraDuration, enemyMask, ctx.owner);
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return;
        ApplyUpgrade(cfg, level);
        level++;
    }

    private void ApplyUpgrade(UpgradeConfig cfg, int seed)
    {
        var stats = UpgradeHelper.GetRandomStats(cfg.count, 2, seed);
        foreach (int stat in stats)
        {
            switch (stat)
            {
                case 0: baseDamage *= cfg.dmgMultiplier; break;
                case 1: 
                    baseSize *= cfg.sizeMultiplier;
                    baseRange *= cfg.sizeMultiplier;
                    break;
            }
        }
    }

    public override List<string> GetUpgradePreview(Rarity rarity, int seed)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return new List<string> { "Unknown" };

        var stats = UpgradeHelper.GetRandomStats(cfg.count, 2, seed);
        var preview = new List<string>();
        
        foreach (int stat in stats)
        {
            preview.Add(stat switch
            {
                0 => $"{UpgradeHelper.FormatPercent(cfg.dmgMultiplier)} Damage",
                1 => $"{UpgradeHelper.FormatPercent(cfg.sizeMultiplier)} AOE",
                _ => "Unknown"
            });
        }
        
        return preview;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return "Unknown";
        return $"Random {cfg.count} of: Damage %, AOE Size";
    }
}