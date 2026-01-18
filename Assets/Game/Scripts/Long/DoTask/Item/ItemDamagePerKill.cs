using UnityEngine;

[CreateAssetMenu(fileName = "ItemDamagePerKill", menuName = "Item/DamagePerKill")]
public class ItemDamagePerKill : Item
{
    [Header("Stats")]
    [Tooltip("Tăng % damage mỗi lần giết enemy")]
    public float damagePercentPerKill = 0.1f; // 0.1%
    
    [Tooltip("Số % damage tối đa")]
    public float maxDamagePercent = 100f; // 100%

    // Listener để track kills
    // private void OnEnable()
    // {
    //     // Subscribe to kill event khi item được tạo
    //     if (GameEventManager.Instance != null)
    //     {
    //         GameEventManager.Instance.OnEnemyKilled += OnEnemyKilled;
    //     }
    // }

    // private void OnDisable()
    // {
    //     // Unsubscribe khi item bị destroy
    //     if (GameEventManager.Instance != null)
    //     {
    //         GameEventManager.Instance.OnEnemyKilled -= OnEnemyKilled;
    //     }
    // }

    private void OnEnemyKilled(object sender, System.EventArgs e)
    {
        var stats = PlayerStatManager.Instance;
        if (stats == null) return;

        // Lấy số lượng item này trong inventory
        int itemCount = 0;
        if (InventoryManager.Instance != null && 
            InventoryManager.Instance.ownedItems.TryGetValue(this, out itemCount))
        {
            if (itemCount > 0)
            {
                float damageIncrease = damagePercentPerKill * itemCount;
                // float newBonus = Mathf.Min(stats.damagePerKillBonus + damageIncrease, 
                //                           maxDamagePercent * itemCount);
                
                // stats.damagePerKillBonus = newBonus;
                // stats.damageMultiplier = 1f + (stats.damagePerKillBonus / 100f);
                
                // Debug.Log($"<color=yellow>[Item]</color> Kill bonus: +{stats.damagePerKillBonus:F1}% damage " +
                //          $"(max {maxDamagePercent * itemCount}%)");
            }
        }
    }

    public override void Apply(int multiplier = 1)
    {
        // Khi item được thêm, subscribe vào kill event
        if (GameEventManager.Instance != null)
        {
            // GameEventManager.Instance.OnEnemyKilled += OnEnemyKilled;
        }
        
        Debug.Log($"<color=green>[Item]</color> Applied Damage Per Kill item (x{multiplier}) - " +
                 $"Max bonus: {maxDamagePercent * multiplier}%");
    }

    public override void Remove(int multiplier = 1)
    {
        // Reset damage bonus khi item bị remove
        float maxBonus = maxDamagePercent * multiplier;
        // stats.damagePerKillBonus = Mathf.Max(0f, stats.damagePerKillBonus - maxBonus);
        // stats.damageMultiplier = 1f + (stats.damagePerKillBonus / 100f);
        
        Debug.Log($"<color=red>[Item]</color> Removed Damage Per Kill item (x{multiplier})");
    }
}