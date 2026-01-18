using UnityEngine;

[CreateAssetMenu(fileName = "ItemMoveSpeed", menuName = "Item/ItemIncreaseMoveSpeed")]
public class ItemIncreaseMoveSpeed : Item
{
    [Header("Stats")]
    [Tooltip("Tăng % tốc độ di chuyển")]
    public float moveSpeedPercentIncrease = 10f; // 10%

    public override void Apply( int multiplier = 1)
    {
        float totalPercent = moveSpeedPercentIncrease * multiplier;
        PlayerStatManager.Instance.ModifyMoveSpeed(1f + totalPercent / 100f);
        
        Debug.Log($"<color=green>[Item]</color> Applied +{totalPercent}% Move Speed (x{multiplier})");
    }

    public override void Remove( int multiplier = 1)
    {
        float totalPercent = moveSpeedPercentIncrease * multiplier;
        // stats.moveSpeedMultiplier /= (1f + totalPercent / 100f);
        
        Debug.Log($"<color=red>[Item]</color> Removed +{totalPercent}% Move Speed (x{multiplier})");
    }
}