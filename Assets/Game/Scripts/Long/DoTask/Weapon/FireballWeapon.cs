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
        Collider[] potentialEnemies =
            Physics.OverlapSphere(ctx.owner.position, targetingRadius, enemyMask);

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

        // 1️⃣ TÍNH SPAWN POS TRƯỚC
        PlayerAttack playerAttack = ctx.owner.GetComponent<PlayerAttack>();
        Vector3 spawnPos = playerAttack != null
            ? playerAttack.ComputeSpawnPosition(ctx.forward)
            : ctx.spawnPos;

        // 2️⃣ TÍNH DIRECTION 3D
        Vector3 direction;

        if (nearestEnemy != null)
        {
            Collider enemyCol = nearestEnemy.GetComponent<Collider>();
            Vector3 targetPos = enemyCol != null
                ? enemyCol.bounds.center
                : nearestEnemy.position;

            direction = (targetPos - spawnPos).normalized;
        }
        else
        {
            direction = ctx.forward.normalized;
        }

        // 3️⃣ ROTATION ĐÚNG CHUẨN 3D
        Quaternion rot = Quaternion.LookRotation(direction);

        GameObject fireballGO = Instantiate(fireballPrefab, spawnPos, rot);

        FireballProjectile proj = fireballGO.GetComponent<FireballProjectile>();
        if (proj != null)
        {
            proj.Initialize(baseDamage, baseRange, baseSize * 25f, enemyMask);
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                baseDamage += 2f;
                baseRange += 0.2f;
                baseCooldown *= 0.98f;  // Giảm cooldown nhẹ
                baseSize += 1f;
                break;

            case Rarity.Uncommon:
                baseDamage += 4f;
                baseRange += 0.3f;
                baseCooldown *= 0.96f;
                baseSize += 1f;
                break;

            case Rarity.Rare:
                baseDamage += 7f;
                baseRange += 0.5f;
                baseCooldown *= 0.93f;
                baseSize += 1.5f;
                break;

            case Rarity.Epic:
                baseDamage += 12f;
                baseRange += 0.8f;
                baseCooldown *= 0.90f;
                baseSize += 2f;
                break;

            case Rarity.Legendary:
                baseDamage += 20f;
                baseRange += 1.2f;
                baseCooldown *= 0.85f; 
                baseSize += 3f;
                break;
        }

        level++;
    }
}