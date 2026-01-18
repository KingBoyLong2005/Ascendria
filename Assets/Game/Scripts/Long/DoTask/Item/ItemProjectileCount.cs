using UnityEngine;

[CreateAssetMenu(fileName = "ItemProjectileCount", menuName = "Item/ProjectileCount")]
public class ItemProjectileCount : Item
{
    [Header("Stats")]
    [Tooltip("Số projectile thêm")]
    public int extraProjectileCount = 1;
    public override void Apply( int multiplier = 1)
    {
        int totalProjectiles = extraProjectileCount * multiplier;
        // stats.bonusProjectileCount += totalProjectiles;
        // Add projectile cho TẤT CẢ weapon có IMultiProjectile
        foreach (var weapon in WeaponManager.Instance.weapons) // Giả sử có list này
        {
            if (weapon is IMultiProjectile multi)
            {
                multi.AddProjectile(totalProjectiles);
                Debug.Log($"Added {totalProjectiles} projectiles to {weapon.name}");
            }
        }
        Debug.Log($"<color=green>[Item]</color> Applied +{totalProjectiles} Projectile Count (x{multiplier})");
    }

    public override void Remove( int multiplier = 1)
    {
        int totalProjectiles = extraProjectileCount * multiplier;
        // stats.bonusProjectileCount -= totalProjectiles;
        // stats.bonusProjectileCount = Mathf.Max(0, stats.bonusProjectileCount);
        
        Debug.Log($"<color=red>[Item]</color> Removed +{totalProjectiles} Projectile Count (x{multiplier})");
    }
}