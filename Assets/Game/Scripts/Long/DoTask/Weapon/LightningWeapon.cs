using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Weapons/Lightning")]
public class LightningWeapon : Weapon
{
    public GameObject lightningPrefab;
    public LayerMask enemyMask;
    public float targetingRadius = 15f;

    public int projectileCount = 1;

    private struct UpgradeConfig
    {
        public int count;
        public float damage, projectiles, sizeMultiplier;
        public UpgradeConfig(int c, float dmg, float proj, float size)
        {
            count = c; damage = dmg; projectiles = proj; sizeMultiplier = size;
        }
    }

    private static readonly Dictionary<Rarity, UpgradeConfig> configs = new()
    {
        { Rarity.Common,     new(1, 2f, 1f, 1.20f) },
        { Rarity.Uncommon,   new(1, 2.4f, 1f, 1.24f) },
        { Rarity.Rare,       new(2, 2.8f, 1f, 1.28f) },
        { Rarity.Epic,       new(2, 3.2f, 2f, 1.32f) },
        { Rarity.Legendary,  new(2, 4f, 2f, 1.40f) }
    };

    public override void Attack(WeaponContext ctx)
    {
        Collider[] enemies = Physics.OverlapSphere(ctx.owner.position, targetingRadius, enemyMask);
        if (enemies.Length == 0) return;

        int strikeCount = Mathf.Min(projectileCount, enemies.Length);
        var enemyList = new List<Collider>(enemies);
        
        for (int i = 0; i < strikeCount; i++)
        {
            if (enemyList.Count == 0) break;
            int idx = Random.Range(0, enemyList.Count);
            SpawnLightning(enemyList[idx].transform);
            enemyList.RemoveAt(idx);
        }
    }

    private void SpawnLightning(Transform enemy)
    {
        Vector3 pos = enemy.position + Vector3.up * 10f;
        
        GameObject go = PoolManager.Spawn(lightningPrefab, pos, lightningPrefab.transform.rotation);
        
        if (go != null)
        {
            go.transform.localScale = Vector3.one * baseSize;
            go.GetComponent<LightningStrike>()?.Initialize(baseDamage, baseRange, enemyMask);
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
        var stats = UpgradeHelper.GetRandomStats(cfg.count, 3, seed);
        foreach (int stat in stats)
        {
            switch (stat)
            {
                case 0: baseDamage += cfg.damage; break;
                case 1: projectileCount += Mathf.RoundToInt(cfg.projectiles); break;
                case 2: 
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

        var stats = UpgradeHelper.GetRandomStats(cfg.count, 3, seed);
        var preview = new List<string>();
        
        foreach (int stat in stats)
        {
            preview.Add(stat switch
            {
                0 => $"+{cfg.damage} Damage",
                1 => $"+{Mathf.RoundToInt(cfg.projectiles)} Strike",
                2 => $"{UpgradeHelper.FormatPercent(cfg.sizeMultiplier)} AOE",
                _ => "Unknown"
            });
        }
        
        return preview;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return "Unknown";
        return $"Random {cfg.count} of: Damage, Strikes, AOE Size";
    }

    // ==================== PROJECTILE MANAGEMENT ====================
    public override bool TryAddProjectile(int amount)
    {
        projectileCount += amount;
        Debug.Log($"<color=cyan>⚡ {weaponName}: {projectileCount - amount} → {projectileCount} strikes</color>");
        return true;
    }

    public override bool TryRemoveProjectile(int amount)
    {
        int oldCount = projectileCount;
        projectileCount = Mathf.Max(1, projectileCount - amount);
        Debug.Log($"<color=orange>⚡ {weaponName}: {oldCount} → {projectileCount} strikes</color>");
        return true;
    }
}