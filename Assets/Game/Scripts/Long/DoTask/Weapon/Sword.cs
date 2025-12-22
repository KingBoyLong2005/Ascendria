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
        Vector3 sizeBox = new Vector3(size, 0.25f, range);

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
            Destroy(fx, 0.4f);
        }
    }
    private void OnHitEnemy(Collider col)
    {
        var enemy = col.GetComponentInParent<EnemyStats>();
        if (enemy != null)
            WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, damage);
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

