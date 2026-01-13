using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Lightning")]
public class LightningWeapon : Weapon
{
    [Header("Lightning")]
    public GameObject lightningPrefab;
    public LayerMask enemyMask;

    [Header("Targeting")]
    public float targetingRadius = 15f;
    public int maxTargets = 3; // mỗi lần đánh tối đa bao nhiêu enemy

    public override void Attack(WeaponContext ctx)
    {
        Collider[] enemies = Physics.OverlapSphere(
            ctx.owner.position,
            targetingRadius,
            enemyMask
        );

        if (enemies.Length == 0) return;

        int hitCount = 0;

        foreach (Collider col in enemies)
        {
            if (hitCount >= maxTargets) break;

            Transform enemy = col.transform;

            // Spawn sét trên đầu enemy
            Vector3 strikePos = enemy.position + Vector3.up * 10f;
            GameObject lightningGO =
                Instantiate(lightningPrefab, strikePos, lightningPrefab.transform.rotation);

            LightningStrike strike = lightningGO.GetComponent<LightningStrike>();
            if (strike != null)
            {
                strike.Initialize(baseDamage, enemyMask);
            }

            hitCount++;
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                baseDamage += 3f;
                targetingRadius += 0.5f;
                break;

            case Rarity.Uncommon:
                baseDamage += 6f;
                targetingRadius += 1f;
                maxTargets += 1;
                break;

            case Rarity.Rare:
                baseDamage += 10f;
                targetingRadius += 1.5f;
                break;

            case Rarity.Epic:
                baseDamage += 15f;
                targetingRadius += 2f;
                maxTargets += 2;
                break;

            case Rarity.Legendary:
                baseDamage += 25f;
                targetingRadius += 3f;
                maxTargets += 3;
                baseCooldown *= 0.85f;
                break;
        }

        level++;
    }
}
