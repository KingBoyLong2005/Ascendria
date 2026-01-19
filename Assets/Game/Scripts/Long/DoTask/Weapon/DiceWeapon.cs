using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Dice")]
public class DiceWeapon : Weapon, IMultiProjectile
{
    [Header("Dice Stats")]
    private int projectileCount = 1;
    private float projectileSpeed = 10f;
    private float critChance = 0.05f; // 5% base crit chance
    private float critDamage = 2f; // 200% crit damage
    private float permanentCritBonus = 0f; // Crit bonus from rolling 6s
    
    public int ProjectileCount => projectileCount;
    public float TotalCritChance => critChance + permanentCritBonus;

    [Header("Dice Prefab")]
    public GameObject dicePrefab;
    public LayerMask enemyMask;

    [Header("Spread")]
    public float spreadAngle = 15f; // Góc spread khi bắn nhiều projectile

    [Header("Targeting")]
    public float targetingRadius = 20f;

    public override void Attack(WeaponContext ctx)
    {
        // Tìm enemy trong tầm
        Collider[] potentialEnemies =
            Physics.OverlapSphere(ctx.owner.position, targetingRadius, enemyMask);

        bool hasEnemy = potentialEnemies.Length > 0;

        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 direction;

            if (hasEnemy)
            {
                // Chọn enemy ngẫu nhiên
                Transform enemy = potentialEnemies[Random.Range(0, potentialEnemies.Length)].transform;

                // Spawn position (giống Fireball)
                PlayerAttack playerAttack = ctx.owner.GetComponent<PlayerAttack>();
                Vector3 spawnPos = playerAttack != null
                    ? playerAttack.ComputeSpawnPosition(ctx.forward)
                    : ctx.spawnPos;

                // Lấy center collider cho chuẩn
                Collider enemyCol = enemy.GetComponent<Collider>();
                Vector3 targetPos = enemyCol != null
                    ? enemyCol.bounds.center
                    : enemy.position;

                direction = (targetPos - spawnPos).normalized;

                // Spread quanh hướng enemy
                if (projectileCount > 1)
                {
                    float angleOffset =
                        spreadAngle * ((float)i / (projectileCount - 1) - 0.5f);

                    direction = Quaternion.Euler(0f, angleOffset, 0f) * direction;
                }
            }
            else
            {
                // Không có enemy → bắn thẳng
                direction = ctx.forward;

                if (projectileCount > 1)
                {
                    float angleOffset =
                        spreadAngle * ((float)i / (projectileCount - 1) - 0.5f);

                    direction = Quaternion.Euler(0f, angleOffset, 0f) * direction;
                }
            }

            ShootDice(ctx, direction);
        }
    }
    private void ShootDice(WeaponContext ctx, Vector3 direction)
    {
        PlayerAttack playerAttack = ctx.owner.GetComponent<PlayerAttack>();
        Vector3 spawnPos = playerAttack != null
            ? playerAttack.ComputeSpawnPosition(ctx.forward)
            : ctx.spawnPos;

        GameObject diceGO = Instantiate(
            dicePrefab,
            spawnPos,
            Quaternion.LookRotation(direction)
        );

        diceGO.transform.localScale = Vector3.one * baseSize;

        DiceProjectile dice = diceGO.GetComponent<DiceProjectile>();
        if (dice != null)
        {
            dice.Initialize(baseDamage, projectileSpeed, direction, enemyMask, this);
        }
    }

    /// <summary>
    /// Called when a dice rolls a 6
    /// </summary>
    public void OnRolledSix()
    {
        permanentCritBonus += 0.02f; // +2% crit chance per lucky roll
        permanentCritBonus = Mathf.Min(permanentCritBonus, 0.5f); // Cap at 50% bonus
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                UpgradeRandomStats(1, 1f, 0.05f, 0.1f, 1f, 1f, 0.95f);
                break;

            case Rarity.Uncommon:
                UpgradeRandomStats(1, 1.5f, 0.07f, 0.15f, 1.2f, 1f, 0.93f);
                break;

            case Rarity.Rare:
                UpgradeRandomStats(2, 2f, 0.10f, 0.2f, 1.5f, 1f, 0.90f);
                break;

            case Rarity.Epic:
                UpgradeRandomStats(2, 3f, 0.12f, 0.25f, 2f, 2f, 0.88f);
                break;

            case Rarity.Legendary:
                UpgradeRandomStats(3, 4f, 0.15f, 0.3f, 2.5f, 2f, 0.85f);
                break;
        }

        level++;
    }

    private void UpgradeRandomStats(
        int count, 
        float dmgBonus, 
        float critChanceBonus, 
        float critDmgBonus,
        float speedBonus,
        float projCountBonus,
        float cooldownMult)
    {
        // 0: Damage, 1: Crit Chance, 2: Crit Damage, 3: Speed, 4: Projectile Count, 5: Size, 6: Cooldown
        var availableStats = new System.Collections.Generic.List<int> { 0, 1, 2, 3, 4, 5, 6 };
        
        for (int i = 0; i < count && availableStats.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableStats.Count);
            int stat = availableStats[randomIndex];
            availableStats.RemoveAt(randomIndex);

            switch (stat)
            {
                case 0: // Damage
                    baseDamage += dmgBonus;
                    Debug.Log($"<color=yellow>🎲 Dice: +{dmgBonus} Damage (now {baseDamage})</color>");
                    break;
                    
                case 1: // Crit Chance
                    critChance += critChanceBonus;
                    critChance = Mathf.Min(critChance, 1f); // Cap at 100%
                    Debug.Log($"<color=yellow>🎲 Dice: +{critChanceBonus * 100}% Crit Chance (now {TotalCritChance * 100}%)</color>");
                    break;
                    
                case 2: // Crit Damage
                    critDamage += critDmgBonus;
                    Debug.Log($"<color=yellow>🎲 Dice: +{critDmgBonus * 100}% Crit Damage (now {critDamage * 100}%)</color>");
                    break;
                    
                case 3: // Projectile Speed
                    projectileSpeed += speedBonus;
                    Debug.Log($"<color=yellow>🎲 Dice: +{speedBonus} Speed (now {projectileSpeed})</color>");
                    break;
                    
                case 4: // Projectile Count
                    projectileCount += Mathf.RoundToInt(projCountBonus);
                    Debug.Log($"<color=yellow>🎲 Dice: +{projCountBonus} Projectiles (now {projectileCount})</color>");
                    break;
                    
                case 5: // Size
                    baseSize *= 1.15f;
                    Debug.Log($"<color=yellow>🎲 Dice: +15% Size (now {baseSize})</color>");
                    break;
                    
                case 6: // Cooldown
                    baseCooldown *= cooldownMult;
                    Debug.Log($"<color=yellow>🎲 Dice: Cooldown reduced to {baseCooldown}s</color>");
                    break;
            }
        }
    }

    public void AddProjectile(int amount)
    {
        projectileCount += amount;
    }

    // Get stats for UI display
    public string GetStatsInfo()
    {
        return $"Damage: {baseDamage:F1} (x1-6 from dice roll)\n" +
               $"Crit Chance: {TotalCritChance * 100:F1}% ({permanentCritBonus * 100:F0}% from lucky rolls)\n" +
               $"Crit Damage: {critDamage * 100:F0}%\n" +
               $"Projectiles: {projectileCount}\n" +
               $"Speed: {projectileSpeed:F1}\n" +
               $"Size: {baseSize:F2}x\n" +
               $"Cooldown: {Cooldown:F2}s";
    }
}