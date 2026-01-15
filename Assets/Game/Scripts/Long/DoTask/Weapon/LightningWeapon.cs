using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Lightning")]
public class LightningWeapon : Weapon
{
    [Header("Lightning Stats")]
    public int projectileCount = 1;  // Số lượng lightning mỗi lần attack
    
    [Header("Lightning")]
    public GameObject lightningPrefab;
    public LayerMask enemyMask;

    [Header("Targeting")]
    public float targetingRadius = 15f;

    public override void Attack(WeaponContext ctx)
    {
        Collider[] enemies = Physics.OverlapSphere(
            ctx.owner.position,
            targetingRadius,
            enemyMask
        );

        if (enemies.Length == 0) return;

        // Bắn projectileCount lightning mỗi lần attack
        int strikeCount = Mathf.Min(projectileCount, enemies.Length);

        // Shuffle enemies để random target
        System.Collections.Generic.List<Collider> enemyList = 
            new System.Collections.Generic.List<Collider>(enemies);
        
        for (int i = 0; i < strikeCount; i++)
        {
            if (enemyList.Count == 0) break;

            // Chọn enemy ngẫu nhiên
            int randomIndex = Random.Range(0, enemyList.Count);
            Collider enemyCol = enemyList[randomIndex];
            enemyList.RemoveAt(randomIndex);

            Transform enemy = enemyCol.transform;

            // Spawn lightning trên đầu enemy
            Vector3 strikePos = enemy.position + Vector3.up * 10f;
            GameObject lightningGO = Instantiate(
                lightningPrefab, 
                strikePos, 
                lightningPrefab.transform.rotation
            );

            // Scale lightning theo baseSize
            lightningGO.transform.localScale = Vector3.one * baseSize;

            LightningStrike strike = lightningGO.GetComponent<LightningStrike>();
            if (strike != null)
            {
                // Truyền cả baseRange làm AOE radius
                strike.Initialize(baseDamage, baseRange, enemyMask);
            }
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                UpgradeRandomStats(1, 2f, 1f, 1.20f);
                break;

            case Rarity.Uncommon:
                UpgradeRandomStats(1, 2.4f, 1f, 1.24f);
                break;

            case Rarity.Rare:
                UpgradeRandomStats(2, 2.8f, 1f, 1.28f);
                break;

            case Rarity.Epic:
                UpgradeRandomStats(2, 3.2f, 2f, 1.32f);
                break;

            case Rarity.Legendary:
                UpgradeRandomStats(2, 4f, 2f, 1.40f);
                break;
        }

        level++;
    }

    private void UpgradeRandomStats(int count, float dmg, float projCount, float sizeMultiplier)
    {
        var availableStats = new System.Collections.Generic.List<int> { 0, 1, 2 };
        
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
                    baseSize *= sizeMultiplier;
                    baseRange *= sizeMultiplier; // AOE size
                    break;
            }
        }
    }
}