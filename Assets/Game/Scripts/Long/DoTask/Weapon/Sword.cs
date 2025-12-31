using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Sword")]
public class Sword : Weapon
{
    public GameObject slashEffectPrefab;
    public LayerMask enemyMask;
    
    public override void Attack(WeaponContext ctx)
    {
        Vector3 center = ctx.spawnPos;
        Quaternion rot = Quaternion.LookRotation(ctx.forward, Vector3.up);
        Vector3 sizeBox = new Vector3(baseSize, 0.25f, baseRange);

        // ==== HITBOX ====
        HitBoxManager.Instance.RequestBox(
            center,
            sizeBox,
            rot,
            enemyMask,
            OnHitEnemy
        );

        // ==== EFFECT ====
        if (slashEffectPrefab != null)
        {
            Quaternion fxRot = rot * Quaternion.Euler(90f, 0f, -60f);
            GameObject fx = Instantiate(slashEffectPrefab, center, fxRot);
            fx.transform.localScale = new Vector3(
                baseSize,          // width
                baseSize,          // thickness
                baseRange          // length
            );
            Destroy(fx, 0.4f);
        }
    }
    private void OnHitEnemy(Collider col)
    {
        var enemy = col.GetComponentInParent<EnemyStats>();
        if (enemy != null)
            WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, baseDamage);
    }
    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                baseDamage += 2f;
                baseRange += 0.2f;
                baseCooldown *= 0.98f;  // Giảm baseCooldown nhẹ
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

