using System.Drawing;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Aura")]
public class AuraWeapon : Weapon
{
    [Header("Aura Settings")]
    public LayerMask enemyMask;
    public GameObject auraEffectPrefab;
    public float auraDuration = 0.25f;

    public override void Attack(WeaponContext ctx)
    {
        size = auraEffectPrefab.transform.localScale.x * 0.5f;
        Vector3 center = ctx.owner.position;
        // ===== HITBOX (AOE) =====
        HitBoxManager.Instance.RequestSphere(
            center,
            size,
            enemyMask,
            OnHitEnemy,
            auraDuration
        );

        // ===== VISUAL EFFECT =====
        SpawnAuraEffect(ctx.owner);
    }
    private void SpawnAuraEffect(Transform owner)
    {
        if (auraEffectPrefab == null) return;

        GameObject fx = Instantiate(auraEffectPrefab, owner);
        fx.transform.localPosition = Vector3.zero;

        // float diameter = size * 2f;
        // fx.transform.localScale = new Vector3(diameter, 1f, diameter);

        Destroy(fx, auraDuration);
    }


    private void OnHitEnemy(Collider col)
    {
        var enemy = col.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
            WeaponManager.Instance.WeaponHitEnemy(
                enemy.gameObject,
                damage
            );
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                damage *= 1.4f;
                size *= 1.14f;
                cooldown *= 0.92f;
                break;

            case Rarity.Uncommon:
                damage *= 1.7f;
                size *= 1.17f;
                cooldown *= 0.88f;
                break;

            case Rarity.Rare:
                damage *= 2.0f;
                size *= 1.20f;
                cooldown *= 0.83f;
                break;

            case Rarity.Epic:
                damage *= 2.2f;
                size *= 1.22f;
                cooldown *= 0.78f;
                break;

            case Rarity.Legendary:
                damage *= 2.8f;
                size *= 1.28f;
                cooldown *= 0.70f;
                break;
        }

        level++;
    }
}
