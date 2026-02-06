using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Weapons/Dice")]
public class DiceWeapon : Weapon
{
    [Header("Dice Stats")]
    private float projectileSpeed = 10f;
    private float aoeSize = 1f; // AOE size for critical 20

    [Header("Dice Prefab")]
    public GameObject dicePrefab;
    public LayerMask enemyMask;

    [Header("Spread")]
    public float spreadAngle = 15f;

    [Header("Targeting")]
    public float targetingRadius = 20f;

    public int projectileCount = 1;

    // ==================== UPGRADE CONFIG ====================
    private struct UpgradeConfig
    {
        public int count;
        public float damage, projectiles, size, cooldownMult;

        public UpgradeConfig(int c, float dmg, float proj, float sz, float cd_mult)
        {
            count = c; damage = dmg; projectiles = proj; 
            size = sz; cooldownMult = cd_mult;
        }
    }

    private static readonly Dictionary<Rarity, UpgradeConfig> configs = new()
    {
        { Rarity.Common,     new(1, 2.5f, 1f, 0.16f, 0.95f) },
        { Rarity.Uncommon,   new(1, 3f, 1.2f, 0.19f, 0.93f) },
        { Rarity.Rare,       new(2, 3.5f, 1.4f, 0.22f, 0.90f) },
        { Rarity.Epic,       new(2, 4f, 1.6f, 0.26f, 0.88f) },
        { Rarity.Legendary,  new(3, 5f, 2f, 0.32f, 0.85f) }
    };

    // ==================== ATTACK ====================
    public override void Attack(WeaponContext ctx)
    {
        Collider[] enemies = Physics.OverlapSphere(ctx.owner.position, targetingRadius, enemyMask);
        bool hasEnemy = enemies.Length > 0;

        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 direction = hasEnemy 
                ? GetDirectionWithSpread(enemies, ctx, i)
                : GetForwardWithSpread(ctx.forward, i);
            
            ShootDice(ctx, direction);
        }
    }

    private Vector3 GetDirectionWithSpread(Collider[] enemies, WeaponContext ctx, int index)
    {
        Transform enemy = enemies[Random.Range(0, enemies.Length)].transform;
        Vector3 spawnPos = GetSpawnPos(ctx);
        Vector3 targetPos = GetEnemyCenter(enemy);
        Vector3 direction = (targetPos - spawnPos).normalized;
        
        return ApplySpread(direction, index);
    }

    private Vector3 GetForwardWithSpread(Vector3 forward, int index)
    {
        return ApplySpread(forward, index);
    }

    private Vector3 ApplySpread(Vector3 direction, int index)
    {
        if (projectileCount <= 1) return direction;
        
        float offset = spreadAngle * ((float)index / (projectileCount - 1) - 0.5f);
        return Quaternion.Euler(0f, offset, 0f) * direction;
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

    private void ShootDice(WeaponContext ctx, Vector3 direction)
    {
        Vector3 spawnPos = GetSpawnPos(ctx);
        Quaternion rotation = Quaternion.LookRotation(direction);
        
        GameObject go = PoolManager.Spawn(dicePrefab, spawnPos, rotation);
        
        if (go != null)
        {
            go.transform.localScale = Vector3.one * baseSize;
            
            float luckValue = PlayerStatManager.Instance.Luck;;
            
            go.GetComponent<DiceProjectile>()?.Initialize(
                baseDamage, 
                projectileSpeed, 
                direction, 
                enemyMask, 
                aoeSize,
                luckValue
            );
        }
    }

    // private float GetPlayerLuck(WeaponContext ctx)
    // {
    //     // var playerStats = ctx.owner.GetComponent<PlayerStatManager>();
    //     // if (playerStats != null)
    //     // {
    //         return PlayerStatManager.Instance.Luck;
    //     // }
    //     // return 0f; // Default luck if PlayerStats not found
    // }

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
                case 2: aoeSize += cfg.size; break;
                case 3: baseCooldown *= cfg.cooldownMult; break;
            }
        }
    }

    // ==================== PREVIEW ====================
    public override List<string> GetUpgradePreview(Rarity rarity, int seed)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return new List<string> { "Unknown" };

        var stats = UpgradeHelper.GetRandomStats(cfg.count, 4, seed);
        var preview = new List<string>();
        
        foreach (int stat in stats)
        {
            preview.Add(stat switch
            {
                0 => $"+{cfg.damage} Damage",
                1 => $"+{Mathf.RoundToInt(cfg.projectiles)} Projectile",
                2 => $"+{cfg.size * 100:F0}% AOE Size",
                3 => $"{UpgradeHelper.FormatPercent(cfg.cooldownMult)} Cooldown",
                _ => "Unknown"
            });
        }
        
        return preview;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return "Unknown";

        return $"Random {cfg.count} of: Damage, Projectile Count, AOE Size, Attack Speed";
    }

    // ==================== PROJECTILE MANAGEMENT ====================
    public override bool TryAddProjectile(int amount)
    {
        projectileCount += amount;
        Debug.Log($"<color=cyan>🎲 {weaponName}: {projectileCount - amount} → {projectileCount} projectiles</color>");
        return true;
    }

    public override bool TryRemoveProjectile(int amount)
    {
        int oldCount = projectileCount;
        projectileCount = Mathf.Max(1, projectileCount - amount);
        Debug.Log($"<color=orange>🎲 {weaponName}: {oldCount} → {projectileCount} projectiles</color>");
        return true;
    }
}