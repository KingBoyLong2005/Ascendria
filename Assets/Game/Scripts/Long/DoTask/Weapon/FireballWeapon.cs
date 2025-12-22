using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Fireball")]
public class FireballWeapon : Weapon
{
    [Header("Effects")]
    public GameObject fireballPrefab;
    // public GameObject explosionEffectPrefab;

    [Header("Mask")]
    public LayerMask enemyMask;

    /// <summary>
    /// Dùng cho bắn cầu lửa, AoE explosion khi trúng enemy
    /// </summary>
    public override void Attack(WeaponContext ctx)
    {
        Vector3 spawnPos = ctx.spawnPos;
        Quaternion rot = Quaternion.LookRotation(ctx.forward, Vector3.up);

        GameObject fireballGO = Instantiate(fireballPrefab, spawnPos, rot);
        FireballProjectile proj = fireballGO.GetComponent<FireballProjectile>();
        if (proj != null)
        {
            // damage: sát thương AoE
            // range: explosion radius
            // size * 25f: projectile speed
            // proj.Initialize(damage, range, size * 25f, enemyMask, explosionEffectPrefab);
            proj.Initialize(damage, range, size * 25f, enemyMask);
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                damage += 2f;
                range += 0.2f;
                cooldown *= 0.98f;  // Giảm cooldown nhẹ
                size += 1f;
                break;

            case Rarity.Uncommon:
                damage += 4f;
                range += 0.3f;
                cooldown *= 0.96f;
                size += 1f;
                break;

            case Rarity.Rare:
                damage += 7f;
                range += 0.5f;
                cooldown *= 0.93f;
                size += 1.5f;
                break;

            case Rarity.Epic:
                damage += 12f;
                range += 0.8f;
                cooldown *= 0.90f;
                size += 2f;
                break;

            case Rarity.Legendary:
                damage += 20f;
                range += 1.2f;
                cooldown *= 0.85f; 
                size += 3f;
                break;
        }

        level++;
    }
}