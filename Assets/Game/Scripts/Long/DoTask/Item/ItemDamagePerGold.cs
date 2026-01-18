using UnityEngine;

[CreateAssetMenu(fileName = "ItemDamagePerGold", menuName = "Item/DamagePerGold")]
public class ItemDamagePerGold : Item
{
    [Header("Stats")]
    [Tooltip("Tăng % damage mỗi 10 Gold")]
    public float damagePercentPer10Gold = 1f; // 1% per 10 gold
    
    private float lastGoldAmount = 0f;

    public override void Apply(int multiplier = 1)
    {
        // Update damage bonus dựa trên gold hiện tại
        // UpdateDamageBonus(stats, multiplier);
        
        Debug.Log($"<color=green>[Item]</color> Applied Damage Per Gold item (x{multiplier})");
    }

    public override void Remove(int multiplier = 1)
    {
        // Recalculate damage bonus without this item
        // float currentBonus = CalculateDamageBonus(stats, multiplier);
        // stats.damagePerGoldBonus -= currentBonus;
        // stats.damageMultiplier = 1f + ((stats.damagePerKillBonus + stats.damagePerGoldBonus) / 100f);
        
        Debug.Log($"<color=red>[Item]</color> Removed Damage Per Gold item (x{multiplier})");
    }

    private void Update()
    {
        // Continuously update damage bonus khi gold thay đổi
        var stats = PlayerStatManager.Instance;
        if (stats == null) return;

        // Check nếu gold đã thay đổi
        float currentGold = stats.Coin;
        if (Mathf.Abs(currentGold - lastGoldAmount) >= 10f)
        {
            lastGoldAmount = currentGold;
            
            // Lấy số lượng item trong inventory
            int itemCount = 0;
            if (InventoryManager.Instance != null &&
                InventoryManager.Instance.ownedItems.TryGetValue(this, out itemCount))
            {
                if (itemCount > 0)
                {
                    // UpdateDamageBonus(stats, itemCount);
                }
            }
        }
    }

    // private void UpdateDamageBonus(int multiplier)
    // {
    //     float newBonus = CalculateDamageBonus(stats, multiplier);
        
        // Remove old bonus và thêm new bonus
        // float oldBonus = stats.damagePerGoldBonus;
        // stats.damagePerGoldBonus = newBonus;
        // stats.damageMultiplier = 1f + ((stats.damagePerKillBonus + stats.damagePerGoldBonus) / 100f);
        
        // if (Mathf.Abs(newBonus - oldBonus) > 0.01f)
        // {
        //     Debug.Log($"<color=yellow>[Item]</color> Gold bonus: +{stats.damagePerGoldBonus:F1}% damage " +
        //              $"({stats.Coin} gold)");
        // }
    // }

    // private float CalculateDamageBonus(int multiplier)
    // {
    //     float goldAmount = stats.Coin;
    //     float goldTiers = Mathf.Floor(goldAmount / 10f);
    //     return goldTiers * damagePercentPer10Gold * multiplier;
    // }
}