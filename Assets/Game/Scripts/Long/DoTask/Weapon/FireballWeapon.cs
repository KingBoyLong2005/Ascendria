using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Weapons/Fireball")]
public class FireballWeapon : Weapon
{
    [Header("Fireball Stats")]
    public float projectileSpeed = 10f;

    [Header("Effects")]
    public GameObject fireballPrefab;
    
    [Header("Mask")]
    public LayerMask enemyMask;
    
    [Header("Targeting")]
    public float targetingRadius = 20f;

    public int projectileCount = 1;

    // ==================== UPGRADE CONFIG ====================
    private struct UpgradeConfig
    {
        public int count;
        public float damage, projectiles, speed, sizeMultiplier;

        public UpgradeConfig(int c, float dmg, float proj, float spd, float size)
        {
            count = c; damage = dmg; projectiles = proj; 
            speed = spd; sizeMultiplier = size;
        }
    }

    private static readonly Dictionary<Rarity, UpgradeConfig> configs = new()
    {
        { Rarity.Common,     new(1, 2.5f, 1f, 0.1f, 1.16f) },
        { Rarity.Uncommon,   new(1, 3f, 1f, 0.12f, 1.19f) },
        { Rarity.Rare,       new(2, 3.5f, 1f, 0.14f, 1.22f) },
        { Rarity.Epic,       new(2, 4f, 2f, 0.16f, 1.26f) },
        { Rarity.Legendary,  new(2, 5f, 2f, 0.2f, 1.32f) }
    };

    // ==================== ATTACK ====================
    public override void Attack(WeaponContext ctx)
    {
        Collider[] enemies = Physics.OverlapSphere(ctx.owner.position, targetingRadius, enemyMask);
        
        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 direction = enemies.Length > 0
                ? GetDirectionToEnemy(enemies, ctx)
                : ctx.forward.normalized;
            
            ShootFireball(ctx, direction);
        }
    }

    private Vector3 GetDirectionToEnemy(Collider[] enemies, WeaponContext ctx)
    {
        Transform enemy = enemies[Random.Range(0, enemies.Length)].transform;
        Vector3 spawnPos = GetSpawnPos(ctx);
        Vector3 targetPos = GetEnemyCenter(enemy);
        return (targetPos - spawnPos).normalized;
    }

    private Vector3 GetSpawnPos(WeaponContext ctx)
    {
        var pa = ctx.owner.GetComponent<PlayerAttack>();
        return pa != null ? pa.ComputeSpawnPosition(ctx.forward) : ctx.spawnPos;
    }

    private Vector3 GetEnemyCenter(Transform enemy)
    {
        var col = enemy.GetComponent<Collider>();
        return col != null ? col.bounds.center : enemy.position;
    }

    private void ShootFireball(WeaponContext ctx, Vector3 direction)
    {
        Vector3 spawnPos = GetSpawnPos(ctx);
        Quaternion rotation = Quaternion.LookRotation(direction);
        
        GameObject go = PoolManager.Spawn(fireballPrefab, spawnPos, rotation);
        
        if (go != null)
        {
            go.transform.localScale = Vector3.one * baseSize;
            go.GetComponent<FireballProjectile>()?.Initialize(baseDamage, baseRange, projectileSpeed, enemyMask);
        }
    }

    // ==================== UPGRADE ====================
    public override void LevelUp(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg)) return;
        ApplyUpgrade(cfg, level);
        level++;
    }

    private void ApplyUpgrade(UpgradeConfig cfg, int seed)
    {
        var stats = UpgradeHelper.GetRandomStats(cfg.count, 4, seed);
        
        foreach (int stat in stats)
        {
            switch (stat)
            {
                case 0: baseDamage += cfg.damage; break;
                case 1: projectileCount += Mathf.RoundToInt(cfg.projectiles); break;
                case 2: projectileSpeed += cfg.speed; break;
                case 3: baseSize *= cfg.sizeMultiplier; break;
            }
        }
    }

    // ==================== PREVIEW ====================
    public override List<string> GetUpgradePreview(Rarity rarity, int seed)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return new List<string> { "Unknown upgrade" };

        var stats = UpgradeHelper.GetRandomStats(cfg.count, 4, seed);
        var preview = new List<string>();
        
        foreach (int stat in stats)
        {
            preview.Add(stat switch
            {
                0 => $"+{cfg.damage} Damage",
                1 => $"+{Mathf.RoundToInt(cfg.projectiles)} Projectile",
                2 => $"+{cfg.speed} Speed",
                3 => $"{UpgradeHelper.FormatPercent(cfg.sizeMultiplier)} Size",
                _ => "Unknown"
            });
        }
        
        return preview;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return "Unknown";

        return $"Random {cfg.count} of: Damage, Projectile, Speed, Size";
    }
}