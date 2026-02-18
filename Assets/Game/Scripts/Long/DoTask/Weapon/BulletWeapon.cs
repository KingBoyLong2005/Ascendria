using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Weapons/Bullet")]
public class BulletWeapon : Weapon
{
    [Header("Bullet Stats")]
    public float projectileSpeed = 15f;
    public int maxBounces = 2;
    public int projectileCount = 1;

    [Header("Effects")]
    public GameObject bulletPrefab;
    
    [Header("Masks")]
    public LayerMask enemyMask;
    public LayerMask bounceableMask;
    
    [Header("Targeting")]
    public float targetingRadius = 20f;
    public float spreadAngle = 15f;

    // ==================== UPGRADE CONFIG ====================
    private struct UpgradeConfig
    {
        public int count;
        public float damage, bounces, projectiles, speed;

        public UpgradeConfig(int c, float dmg, float bnc, float proj, float spd)
        {
            count = c; 
            damage = dmg; 
            bounces = bnc; 
            projectiles = proj; 
            speed = spd;
        }
    }

    private static readonly Dictionary<Rarity, UpgradeConfig> configs = new()
    {
        { Rarity.Common,     new(1, 3f,   1f,  0f,  1f) },
        { Rarity.Uncommon,   new(1, 4f,   1f,  1f,  1.5f) },
        { Rarity.Rare,       new(2, 5f,   1f,  1f,  2f) },
        { Rarity.Epic,       new(2, 6f,   2f,  1f,  2.5f) },
        { Rarity.Legendary,  new(2, 8f,   2f,  2f,  3f) }
    };

    // ==================== ATTACK ====================
    public override void Attack(WeaponContext ctx)
    {
        Collider[] enemies = Physics.OverlapSphere(ctx.owner.position, targetingRadius, enemyMask);
        
        // Nếu có enemy → tìm enemy gần nhất
        Transform targetEnemy = null;
        if (enemies.Length > 0)
        {
            targetEnemy = GetNearestEnemy(enemies, ctx.owner.position);
        }

        // Bắn từng viên đạn
        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 direction;

            if (targetEnemy != null)
            {
                // Có enemy → bắn về phía enemy với spread nhỏ
                direction = GetDirectionToEnemy(targetEnemy, ctx, i);
            }
            else
            {
                // Không có enemy → bắn về phía gần nhất theo hướng player đang nhìn
                direction = GetSpreadDirection(ctx.forward, i);
            }
            
            ShootBullet(ctx, direction);
        }
    }

    private Transform GetNearestEnemy(Collider[] enemies, Vector3 fromPosition)
    {
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (Collider col in enemies)
        {
            float dist = Vector3.Distance(fromPosition, col.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = col.transform;
            }
        }

        return nearest;
    }

    private Vector3 GetDirectionToEnemy(Transform enemy, WeaponContext ctx, int bulletIndex)
    {
        Vector3 spawnPos = GetSpawnPos(ctx);
        Vector3 targetPos = GetEnemyCenter(enemy);
        Vector3 baseDirection = (targetPos - spawnPos).normalized;
        
        // Nếu bắn nhiều viên → thêm spread nhỏ xung quanh enemy
        if (projectileCount > 1)
        {
            // Tính góc spread từ -spreadAngle/2 đến +spreadAngle/2
            float normalizedIndex = (float)bulletIndex / Mathf.Max(1, projectileCount - 1); // 0 to 1
            float angle = spreadAngle * (normalizedIndex - 0.5f);
            return Quaternion.Euler(0, angle, 0) * baseDirection;
        }
        
        return baseDirection;
    }

    private Vector3 GetSpreadDirection(Vector3 baseDirection, int bulletIndex)
    {
        if (projectileCount == 1) return baseDirection;
        
        // Spread đều xung quanh hướng base
        float normalizedIndex = (float)bulletIndex / Mathf.Max(1, projectileCount - 1);
        float angle = spreadAngle * (normalizedIndex - 0.5f);
        return Quaternion.Euler(0, angle, 0) * baseDirection;
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

    private void ShootBullet(WeaponContext ctx, Vector3 direction)
    {
        Vector3 spawnPos = GetSpawnPos(ctx);
        Quaternion rotation = Quaternion.LookRotation(direction);
        
        GameObject go = PoolManager.Spawn(bulletPrefab, spawnPos, rotation);
        
        if (go != null)
        {
            // go.transform.localScale = Vector3.one * baseSize;
            var bullet = go.GetComponent<BulletProjectile>();
            bullet?.Initialize(
                baseDamage, 
                projectileSpeed, 
                maxBounces, 
                enemyMask, 
                bounceableMask
            );
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
                case 0: // Damage
                    baseDamage += cfg.damage;
                    break;
                    
                case 1: // Bounces
                    maxBounces += Mathf.RoundToInt(cfg.bounces);
                    break;
                    
                case 2: // Projectiles
                    projectileCount += Mathf.RoundToInt(cfg.projectiles);
                    break;
                    
                case 3: // Speed
                    projectileSpeed += cfg.speed;
                    break;
            }
        }
    }

    // ==================== PROJECTILE SUPPORT ====================
    public override bool TryAddProjectile(int amount)
    {
        projectileCount += amount;
        return true;
    }

    public override bool TryRemoveProjectile(int amount)
    {
        if (projectileCount - amount < 1) return false;
        projectileCount -= amount;
        return true;
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
                1 => $"+{Mathf.RoundToInt(cfg.bounces)} Bounce",
                2 => $"+{Mathf.RoundToInt(cfg.projectiles)} Projectile",
                3 => $"+{cfg.speed} Speed",
                _ => "Unknown"
            });
        }
        
        return preview;
    }

    public override string GetUpgradeDescription(Rarity rarity)
    {
        if (!configs.TryGetValue(rarity, out var cfg))
            return "Unknown";

        return $"Random {cfg.count} of: Damage, Bounce, Projectile, Speed";
    }
}