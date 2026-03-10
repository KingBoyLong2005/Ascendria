using UnityEngine;

[CreateAssetMenu(fileName = "ItemProjectileCount", menuName = "Item/ProjectileCount")]
public class ItemProjectileCount : Item
{
    [Header("Stats")]
    [Tooltip("Số projectile thêm")]
    public int extraProjectileCount = 1;

    public override void Apply(int multiplier = 1)
    {
        int totalProjectiles = extraProjectileCount * multiplier;
        int affectedCount = 0;

        // Thêm projectile cho TẤT CẢ weapon có hỗ trợ projectile
        foreach (var weapon in WeaponManager.Instance.weapons)
        {
            if (weapon.TryAddProjectile(totalProjectiles))
            {
                affectedCount++;
            }
        }

        if (affectedCount > 0)
        {
            Debug.Log($"<color=green>✨ [Item] Applied +{totalProjectiles} Projectile Count to {affectedCount} weapon(s) (x{multiplier})</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>⚠️ [Item] No weapons support projectiles</color>");
        }
    }

    public override void Remove(int multiplier = 1)
    {
        int totalProjectiles = extraProjectileCount * multiplier;
        int affectedCount = 0;

        // Trừ projectile
        foreach (var weapon in WeaponManager.Instance.weapons)
        {
            if (weapon.TryRemoveProjectile(totalProjectiles))
            {
                affectedCount++;
            }
        }

        if (affectedCount > 0)
        {
            Debug.Log($"<color=red>🔻 [Item] Removed -{totalProjectiles} Projectile Count from {affectedCount} weapon(s) (x{multiplier})</color>");
        }
    }
}