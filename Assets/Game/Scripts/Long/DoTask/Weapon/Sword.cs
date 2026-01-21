using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Sword")]
public class Sword : Weapon, IMultiProjectile
{
    [Header("Sword Stats")]
    private int projectileCount = 1;
    
    public int ProjectileCount => projectileCount;
    public float knockbackForce = 0f;    // Lực đẩy lùi enemy
    
    [Header("Effects")]
    public GameObject slashEffectPrefab;
    
    [Header("Mask")]
    public LayerMask enemyMask;
    public override void Attack(WeaponContext ctx)
    {
        // Tấn công projectileCount lần
        for (int i = 0; i < projectileCount; i++)
        {
            // Random angle cho mỗi slash nếu có nhiều slash
            float angleOffset = 0f;
            if (projectileCount > 1)
            {
                // Spread các slash trong khoảng ±30 độ
                float spreadAngle = 60f;
                angleOffset = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
            }

            // Tính rotation với offset
            Quaternion rot = Quaternion.LookRotation(ctx.forward, Vector3.up) * 
                            Quaternion.Euler(0f, angleOffset, 0f);
            
            Vector3 center = ctx.spawnPos;
            Vector3 sizeBox = new Vector3(baseSize, 0.25f, baseRange);

            // ==== HITBOX ====
            HitBoxManager.Instance.RequestBox(
                center,
                sizeBox,
                rot,
                enemyMask,
                // OnHitEnemy,
                (col) => OnHitEnemy(col, ctx)
            );

            // ==== EFFECT ====
            if (slashEffectPrefab != null)
            {
                Quaternion fxRot = rot * Quaternion.Euler(90f, 0f, -60f);
                GameObject fx = Instantiate(slashEffectPrefab, center, fxRot);
                
                // Scale effect theo baseSize và baseRange
                fx.transform.localScale = new Vector3(
                    baseSize,          // width
                    baseSize,          // thickness
                    baseRange          // length
                );
                
                Destroy(fx, 0.4f);
            }

            // Delay nhỏ giữa các slash nếu có nhiều slash
            if (projectileCount > 1 && i < projectileCount - 1)
            {
                // Có thể thêm delay ở đây nếu cần
            }
        }
    }

    private void OnHitEnemy(Collider col, WeaponContext playerctx)
    {
        var enemy = col.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
            WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, baseDamage);
            
            // Apply knockback nếu có
            if (knockbackForce > 0f)
            {
                ApplyKnockback(enemy, playerctx);
            }
        }
    }

    private void ApplyKnockback(EnemyStats enemy, WeaponContext playerctx)
    {
        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            // Tính hướng knockback (đẩy ra xa player)
            Vector3 knockbackDir = (enemy.transform.position - playerctx.owner.transform.position).normalized;
            knockbackDir.y = 0f; // Giữ knockback ở mặt phẳng ngang
            
            // Apply force
            enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                UpgradeRandomStats(1, 2f, 1f, 0.5f, 1.20f);
                break;

            case Rarity.Uncommon:
                UpgradeRandomStats(1, 2.4f, 1f, 0.6f, 1.24f);
                break;

            case Rarity.Rare:
                UpgradeRandomStats(2, 2.8f, 1f, 0.7f, 1.28f);
                break;

            case Rarity.Epic:
                UpgradeRandomStats(2, 3.2f, 2f, 0.8f, 1.32f);
                break;

            case Rarity.Legendary:
                UpgradeRandomStats(2, 4f, 2f, 1f, 1.40f);
                break;
        }

        level++;
    }

    private void UpgradeRandomStats(int count, float dmg, float projCount, float knockback, float sizeMultiplier)
    {
        var availableStats = new System.Collections.Generic.List<int> { 0, 1, 2, 3 };
        
        for (int i = 0; i < count && availableStats.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableStats.Count);
            int stat = availableStats[randomIndex];
            availableStats.RemoveAt(randomIndex);

            switch (stat)
            {
                case 0: 
                    baseDamage += dmg; 
                    break;
                case 1: 
                    projectileCount += Mathf.RoundToInt(projCount); 
                    break;
                case 2: 
                    knockbackForce += knockback; 
                    break;
                case 3: 
                    baseSize *= sizeMultiplier;
                    baseRange *= sizeMultiplier;
                    break;
            }
        }
    }
    public void AddProjectile(int amount)
    {
        projectileCount += amount;
    }
}