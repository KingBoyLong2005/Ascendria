using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Aura")]
public class AuraWeapon : Weapon
{
    [Header("Aura Stats")]
    public float auraDuration = 10f;     // Thời gian aura tồn tại
    
    [Header("Effects")]
    public GameObject auraPrefab;        // Effect visual cho aura
    
    [Header("Mask")]
    public LayerMask enemyMask;

    public override void Attack(WeaponContext ctx)
    {
        // Spawn aura tại vị trí player
        Vector3 spawnPos = ctx.owner.position;
        
        GameObject auraGO = Instantiate(auraPrefab, spawnPos, auraPrefab.transform.rotation);
        
        // Set parent để aura follow player
        auraGO.transform.SetParent(ctx.owner);
        auraGO.transform.localPosition = Vector3.zero;
        
        // Scale aura theo baseSize
        auraGO.transform.localScale = Vector3.one * baseSize;
        
        // Initialize aura component
        AuraEffect aura = auraGO.GetComponent<AuraEffect>();
        if (aura != null)
        {
            aura.Initialize(baseDamage, baseRange, auraDuration, enemyMask, ctx.owner);
        }
        else
        {
            Debug.LogWarning("AuraPrefab cần có component AuraEffect!");
        }
    }

    public override void LevelUp(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                UpgradeRandomStats(1, 1.014f, 2.14f);  // 1.4% damage, 214% size
                break;

            case Rarity.Uncommon:
                UpgradeRandomStats(1, 1.017f, 1.17f);  // 1.7% damage, 17% size
                break;

            case Rarity.Rare:
                UpgradeRandomStats(2, 1.02f, 1.20f);   // 2% damage, 20% size
                break;

            case Rarity.Epic:
                UpgradeRandomStats(2, 1.022f, 1.22f);  // 2.2% damage, 22% size
                break;

            case Rarity.Legendary:
                UpgradeRandomStats(2, 1.028f, 1.28f);  // 2.8% damage, 28% size
                break;
        }

        level++;
    }

    private void UpgradeRandomStats(int count, float dmgMultiplier, float sizeMultiplier)
    {
        var availableStats = new System.Collections.Generic.List<int> { 0, 1 };
        
        for (int i = 0; i < count && availableStats.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableStats.Count);
            int stat = availableStats[randomIndex];
            availableStats.RemoveAt(randomIndex);

            switch (stat)
            {
                case 0: 
                    baseDamage *= dmgMultiplier;  // Damage tăng theo %
                    break;
                case 1: 
                    baseSize *= sizeMultiplier;   // Size tăng theo %
                    baseRange *= sizeMultiplier;  // AOE radius cũng tăng
                    break;
            }
        }
    }
}