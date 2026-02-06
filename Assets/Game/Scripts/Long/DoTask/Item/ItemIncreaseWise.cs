using UnityEngine;

[CreateAssetMenu(fileName = "ItemWise", menuName = "Item/Wise")]
public class ItemWise : Item
{
    [Header("Stats")]
    [Tooltip("Tăng % giá trị XP Gem")]
    public float xpPercentIncrease = 5f; // 5%

    public override void Apply( int multiplier = 1)
    {
        float totalPercent = xpPercentIncrease * multiplier;
        PlayerStatManager.Instance.ModifyWise(1f + totalPercent / 100f);
        
        Debug.Log($"<color=green>[Item]</color> Applied +{totalPercent}% Wise (XP) (x{multiplier})");
    }

    public override void Remove( int multiplier = 1)
    {
        float totalPercent = xpPercentIncrease * multiplier;
        // stats.xpMultiplier /= (1f + totalPercent / 100f);
        
        Debug.Log($"<color=red>[Item]</color> Removed +{totalPercent}% Wise (XP) (x{multiplier})");
    }
}