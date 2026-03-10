using System.Collections.Generic;
using UnityEngine;

public struct WeaponContext
{
    public Vector3 spawnPos;
    public Vector3 forward;
    public Transform owner;
}

public abstract class Weapon : UpgradableItem
{
    public string weaponName = "DefaultWeapon";
    
    [Header("Stats")]
    public float baseDamage = 10f;
    public float baseRange = 2f;
    public float baseSize = 1f;
    public float baseCooldown = 1f;
    
    protected float timer = 0f;
    protected float cooldownMultiplier = 1f;
    
    public float Cooldown => baseCooldown * cooldownMultiplier;

    public void Tick(float dt, WeaponContext ctx)
    {
        timer += dt;
        if (timer >= Cooldown)
        {
            timer = 0f;
            Attack(ctx);
        }
    }

    public abstract void Attack(WeaponContext ctx);
    
    public void ApplyCooldownReduction(float percent)
    {
        cooldownMultiplier *= (1f - percent);
        cooldownMultiplier = Mathf.Clamp(cooldownMultiplier, 0.1f, 1f);
    }

    /// <summary>
    /// Thêm projectile (chỉ weapon nào có projectileCount mới override)
    /// </summary>
    public virtual bool TryAddProjectile(int amount)
    {
        return false; // Default: weapon không hỗ trợ projectile
    }

    /// <summary>
    /// Trừ projectile (chỉ weapon nào có projectileCount mới override)
    /// </summary>
    public virtual bool TryRemoveProjectile(int amount)
    {
        return false; // Default: weapon không hỗ trợ projectile
    }

    // ==================== UPGRADE PREVIEW ====================
    /// <summary>
    /// Trả về danh sách các stat sẽ được upgrade (đã random)
    /// </summary>
    // public virtual List<string> GetUpgradePreview(Rarity rarity, int seed)
    // {
    //     return new List<string> { "No preview available" };
    // }
}
