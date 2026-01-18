using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseDamage", menuName = "Item/ItemIncreaseDamage")]
public class ItemIncreaseDamage : Item
{
    [Header("Stats")]
    [Tooltip("Tăng % damage")]
    public float damagePercentIncrease = 10f; // 10%

    public override void Apply(int multiplier = 1)
    {
        // Tăng damage theo % với multiplier (số lượng stack)
        float totalPercent = damagePercentIncrease * multiplier;
        PlayerStatManager.Instance.ModifyAttack(1f + totalPercent / 100f);
        
        Debug.Log($"<color=green>[Item]</color> Applied +{totalPercent}% Damage (x{multiplier})");
    }

    public override void Remove(int multiplier = 1)
    {
        // Giảm damage theo %
        float totalPercent = damagePercentIncrease * multiplier;
        // stats.damageMultiplier /= (1f + totalPercent / 100f);
        
        Debug.Log($"<color=red>[Item]</color> Removed +{totalPercent}% Damage (x{multiplier})");
    }
}