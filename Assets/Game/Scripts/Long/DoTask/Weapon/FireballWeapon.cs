using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Fireball")]
public class FireballWeapon : Weapon
{
    [Header("Effects")]
    public GameObject fireballPrefab;
    // public GameObject explosionEffectPrefab;

    [Header("Mask")]
    public LayerMask enemyMask;

    [Header("Targeting")]
    public float targetingRadius = 20f;  // Bán kính tìm kiếm enemy gần nhất

    /// <summary>
    /// Dùng cho bắn cầu lửa, AoE explosion khi trúng enemy
    /// </summary>
    public override void Attack(WeaponContext ctx)
    {
        // Tìm enemy gần nhất trong bán kính targetingRadius
        Collider[] potentialEnemies = Physics.OverlapSphere(ctx.owner.position, targetingRadius, enemyMask);
        Transform nearestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var col in potentialEnemies)
        {
            float dist = Vector3.Distance(ctx.owner.position, col.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestEnemy = col.transform;
            }
        }

        Vector3 direction;
        if (nearestEnemy != null)
        {
            // Hướng từ player đến enemy (flat để tránh lên/xuống)
            Vector3 toEnemy = nearestEnemy.position - ctx.owner.position;
            direction = new Vector3(toEnemy.x, 0f, toEnemy.z).normalized;
        }
        else
        {
            // Nếu không có enemy, dùng hướng mặc định
            direction = ctx.forward;
        }

        // Tính spawnPos dựa trên hướng mới (sử dụng ComputeSpawnPosition từ PlayerAttack)
        PlayerAttack playerAttack = ctx.owner.GetComponent<PlayerAttack>();
        Vector3 spawnPos = playerAttack != null ? playerAttack.ComputeSpawnPosition(direction) : ctx.spawnPos;

        // Spawn fireball với rotation theo hướng
        Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);

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