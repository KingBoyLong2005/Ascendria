using UnityEngine;

[CreateAssetMenu(fileName = "ItemWealth", menuName = "Item/ItemIncreaseWealth")]
public class ItemWealth : Item
{
    [Header("Stats")]
    [Tooltip("Tăng % giá trị Gold và Silver")]
    public float wealthPercentIncrease = 5f; // 5%

    public override void Apply( int multiplier = 1)
    {
        float totalPercent = wealthPercentIncrease * multiplier;
        // stats.ModifyWealth(1f + totalPercent / 100f);
        
        Debug.Log($"<color=green>[Item]</color> Applied +{totalPercent}% Wealth (x{multiplier})");
    }

    public override void Remove( int multiplier = 1)
    {
        float totalPercent = wealthPercentIncrease * multiplier;
        // stats.goldMultiplier /= (1f + totalPercent / 100f);
        
        Debug.Log($"<color=red>[Item]</color> Removed +{totalPercent}% Wealth (x{multiplier})");
    }
}