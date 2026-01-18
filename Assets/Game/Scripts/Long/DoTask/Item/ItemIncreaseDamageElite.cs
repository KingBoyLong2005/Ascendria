using UnityEngine;

[CreateAssetMenu(fileName = "ItemIncreaseDamageElite", menuName = "Item/ItemDamageElite")]
public class ItemEliteDamage : Item
{
    [Header("Stats")]
    [Tooltip("Tăng % damage lên Elite và Boss")]
    public float eliteDamagePercentIncrease = 10f; // 10%

    public override void Apply(int multiplier = 1)
    {
        float totalPercent = eliteDamagePercentIncrease * multiplier;
        // stats.eliteDamageMultiplier *= (1f + totalPercent / 100f);
        
        Debug.Log($"<color=green>[Item]</color> Applied +{totalPercent}% Elite/Boss Damage (x{multiplier})");
    }

    public override void Remove(int multiplier = 1)
    {
        float totalPercent = eliteDamagePercentIncrease * multiplier;
        // stats.eliteDamageMultiplier /= (1f + totalPercent / 100f);
        
        Debug.Log($"<color=red>[Item]</color> Removed +{totalPercent}% Elite/Boss Damage (x{multiplier})");
    }
}