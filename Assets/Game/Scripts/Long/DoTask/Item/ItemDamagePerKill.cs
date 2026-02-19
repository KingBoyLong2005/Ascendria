using UnityEngine;

[CreateAssetMenu(fileName = "ItemDamagePerKill", menuName = "Item/DamagePerKill")]
public class ItemDamagePerKill : Item
{
    [Header("Stats")]
    [Tooltip("Tăng % damage mỗi lần giết enemy (flat addFlat vào ModifyDamage)")]
    public float damagePercentPerKill = 0.1f; // 0.1% mỗi kill

    [Tooltip("Số % damage tối đa có thể tích lũy (per stack)")]
    public float maxDamagePercent = 100f; // 100%

    // Track tổng bonus đã cộng để kiểm tra cap
    private float totalAppliedBonus = 0f;

    public override void Apply(int multiplier = 1)
    {
        // Subscribe vào OnDead khi item được equip
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnDead += OnEnemyKilled;
        }

        Debug.Log($"<color=green>[ItemDamagePerKill]</color> Applied x{multiplier} - " +
                  $"Max bonus: {maxDamagePercent * multiplier}%");
    }

    public override void Remove(int multiplier = 1)
    {
        // Unsubscribe khi item bị remove
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnDead -= OnEnemyKilled;
        }

        // Trừ lại bonus đã cộng vào
        if (totalAppliedBonus > 0f && PlayerStatManager.Instance != null)
        {
            PlayerStatManager.Instance.ModifyDamage(addFlat: -totalAppliedBonus);
            Debug.Log($"<color=red>[ItemDamagePerKill]</color> Removed bonus: -{totalAppliedBonus:F2}%");
        }

        totalAppliedBonus = 0f;

        Debug.Log($"<color=red>[ItemDamagePerKill]</color> Removed x{multiplier}");
    }

    private void OnEnemyKilled(object sender, EnemyManager.OnEnemyDeathEventArgs e)
    {
        var stats = PlayerStatManager.Instance;
        if (stats == null) return;

        int itemCount = InventoryManager.Instance != null
            ? InventoryManager.Instance.GetItemCount(this)
            : 1;

        if (itemCount <= 0) return;

        float cap = maxDamagePercent * itemCount;

        // Chỉ cộng thêm nếu chưa đạt cap
        if (totalAppliedBonus >= cap) return;

        float increase = damagePercentPerKill * itemCount;
        float actualIncrease = Mathf.Min(increase, cap - totalAppliedBonus);

        stats.ModifyDamage(addFlat: actualIncrease);
        totalAppliedBonus += actualIncrease;

        Debug.Log($"<color=yellow>[ItemDamagePerKill]</color> Kill bonus: +{actualIncrease:F2}% " +
                  $"(total: {totalAppliedBonus:F1}% / {cap}%)");
    }
}