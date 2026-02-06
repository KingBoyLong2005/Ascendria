using UnityEngine;

[CreateAssetMenu(fileName = "ItemMaxHP", menuName = "Item/ItemIncreaseMaxHP")]
public class ItemIncreaseMaxHP : Item
{
    [Header("Stats")]
    [Tooltip("Tăng máu tối đa")]
    public float maxHPIncrease = 25f;

    public override void Apply(int multiplier = 1)
    {
        float totalHP = maxHPIncrease * multiplier;
        PlayerStatManager.Instance.ModifyHealth(totalHP);
        
        Debug.Log($"<color=green>[Item]</color> Applied +{totalHP} Max HP (x{multiplier})");
    }

    public override void Remove(int multiplier = 1)
    {
        float totalHP = maxHPIncrease * multiplier;
        PlayerStatManager.Instance.ModifyHealth(-totalHP);
        
        Debug.Log($"<color=red>[Item]</color> Removed +{totalHP} Max HP (x{multiplier})");
    }
}