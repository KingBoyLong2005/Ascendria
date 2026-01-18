using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Fireball")]
public class FireballWeapon : Weapon, IMultiProjectile
{
    [Header("Fireball Stats")]
    public float projectileSpeed = 10f;      // Tốc độ base
    private int projectileCount = 1;
    
    public int ProjectileCount => projectileCount;
    // public int MaxProjectileCount => 10; // Giới hạn      // Số lượng projectile mỗi lần attack
    [Header("Effects")]
    public GameObject fireballPrefab;
    
    [Header("Mask")]
    public LayerMask enemyMask;
    
    [Header("Targeting")]
    public float targetingRadius = 20f;

    /// <summary>
    /// Bắn fireball về phía enemy ngẫu nhiên trong tầm
    /// </summary>
    public override void Attack(WeaponContext ctx)
    {
        // Tìm tất cả enemy trong tầm
        Collider[] potentialEnemies = 
            Physics.OverlapSphere(ctx.owner.position, targetingRadius, enemyMask);

        if (potentialEnemies.Length == 0)
        {
            // Không có enemy -> bắn về phía trước
            ShootFireball(ctx, ctx.forward.normalized);
            return;
        }

        // Bắn nhiều fireball dựa theo projectileCount
        for (int i = 0; i < projectileCount; i++)
        {
            // Chọn enemy ngẫu nhiên
            Transform randomEnemy = potentialEnemies[Random.Range(0, potentialEnemies.Length)].transform;
            
            // Tính spawn position
            PlayerAttack playerAttack = ctx.owner.GetComponent<PlayerAttack>();
            Vector3 spawnPos = playerAttack != null
                ? playerAttack.ComputeSpawnPosition(ctx.forward)
                : ctx.spawnPos;

            // Tính direction về phía enemy
            Collider enemyCol = randomEnemy.GetComponent<Collider>();
            Vector3 targetPos = enemyCol != null 
                ? enemyCol.bounds.center 
                : randomEnemy.position;

            Vector3 direction = (targetPos - spawnPos).normalized;

            ShootFireball(ctx, direction);
        }
    }

    private void ShootFireball(WeaponContext ctx, Vector3 direction)
    {
        // Tính spawn position
        PlayerAttack playerAttack = ctx.owner.GetComponent<PlayerAttack>();
        Vector3 spawnPos = playerAttack != null
            ? playerAttack.ComputeSpawnPosition(ctx.forward)
            : ctx.spawnPos;

        // Tạo rotation
        Quaternion rot = Quaternion.LookRotation(direction);

        // Spawn fireball
        GameObject fireballGO = Instantiate(fireballPrefab, spawnPos, rot);
        
        // Scale fireball dựa theo baseSize
        fireballGO.transform.localScale = Vector3.one * baseSize;

        // Initialize projectile
        FireballProjectile proj = fireballGO.GetComponent<FireballProjectile>();
        if (proj != null)
        {
            proj.Initialize(baseDamage, baseRange, projectileSpeed, enemyMask);
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                UpgradeRandomStats(1, 2.5f, 1f, 0.1f, 1.16f);
                break;

            case Rarity.Uncommon:
                UpgradeRandomStats(1, 3f, 1f, 0.12f, 1.19f);
                break;

            case Rarity.Rare:
                UpgradeRandomStats(2, 3.5f, 1f, 0.14f, 1.22f);
                break;

            case Rarity.Epic:
                UpgradeRandomStats(2, 4f, 2f, 0.16f, 1.26f);
                break;

            case Rarity.Legendary:
                UpgradeRandomStats(2, 5f, 2f, 0.2f, 1.32f);
                break;
        }

        level++;
    }

    private void UpgradeRandomStats(int count, float dmg, float projCount, float projSpeed, float sizeMultiplier)
    {
        // Tạo danh sách các stat có thể upgrade
        var availableStats = new System.Collections.Generic.List<int> { 0, 1, 2, 3 };
        
        for (int i = 0; i < count && availableStats.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableStats.Count);
            int stat = availableStats[randomIndex];
            availableStats.RemoveAt(randomIndex);

            switch (stat)
            {
                case 0: baseDamage += dmg; break;
                case 1: projectileCount += Mathf.RoundToInt(projCount); break;
                case 2: projectileSpeed += projSpeed; break;
                case 3: baseSize *= sizeMultiplier; break;
            }
        }
    }
    public void AddProjectile(int amount)
    {
        projectileCount += amount;
    }
}