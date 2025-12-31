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
        baseSize = auraEffectPrefab.transform.localScale.x * 0.5f;
        Vector3 center = ctx.owner.position;
        // ===== HITBOX (AOE) =====
        HitBoxManager.Instance.RequestSphere(
            center,
            baseSize,
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
                baseDamage
            );
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                baseDamage *= 1.4f;
                baseSize *= 1.14f;
                baseCooldown *= 0.92f;
                break;

            case Rarity.Uncommon:
                baseDamage *= 1.7f;
                baseSize *= 1.17f;
                baseCooldown *= 0.88f;
                break;

            case Rarity.Rare:
                baseDamage *= 2.0f;
                baseSize *= 1.20f;
                baseCooldown *= 0.83f;
                break;

            case Rarity.Epic:
                baseDamage *= 2.2f;
                baseSize *= 1.22f;
                baseCooldown *= 0.78f;
                break;

            case Rarity.Legendary:
                baseDamage *= 2.8f;
                baseSize *= 1.28f;
                baseCooldown *= 0.70f;
                break;
        }

        level++;
    }
}
