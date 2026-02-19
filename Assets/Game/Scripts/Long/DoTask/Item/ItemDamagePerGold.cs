using UnityEngine;

[CreateAssetMenu(fileName = "ItemDamagePerGold", menuName = "Item/DamagePerGold")]
public class ItemDamagePerGold : Item
{
    [Header("Stats")]
    [Tooltip("Tăng % damage mỗi 10 Gold nhận được (không tính gold trước khi có item)")]
    public float damagePercentPer10Gold = 1f; // 1% per 10 gold

    // Gold tại thời điểm item được equip → làm mốc, không tính retroactive
    private float goldAtEquip = 0f;

    // Tổng gold đã được tính vào damage bonus (theo tier)
    private float goldCountedSoFar = 0f;

    // Tổng bonus đã cộng vào để có thể remove sau
    private float totalAppliedBonus = 0f;

    public override void Apply(int multiplier = 1)
    {
        // Lấy gold hiện tại làm mốc — gold trước đó không được tính
        goldAtEquip = InventoryManager.Instance != null
            ? InventoryManager.Instance.GetTotalCoins()
            : 0f;

        goldCountedSoFar = 0f;
        totalAppliedBonus = 0f;

        // Subscribe vào OnDead để check gold sau mỗi kill (gold thường tăng khi kill)
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnDead += OnEnemyKilled;
        }

        Debug.Log($"<color=green>[ItemDamagePerGold]</color> Applied x{multiplier} - " +
                  $"Gold mốc: {goldAtEquip}");
    }

    public override void Remove(int multiplier = 1)
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnDead -= OnEnemyKilled;
        }

        // Trừ lại toàn bộ bonus đã cộng
        if (totalAppliedBonus > 0f && PlayerStatManager.Instance != null)
        {
            PlayerStatManager.Instance.ModifyDamage(addFlat: -totalAppliedBonus);
            Debug.Log($"<color=red>[ItemDamagePerGold]</color> Removed bonus: -{totalAppliedBonus:F2}%");
        }

        goldAtEquip = 0f;
        goldCountedSoFar = 0f;
        totalAppliedBonus = 0f;

        Debug.Log($"<color=red>[ItemDamagePerGold]</color> Removed x{multiplier}");
    }

    private void OnEnemyKilled(object sender, EnemyManager.OnEnemyDeathEventArgs e)
    {
        CheckGoldBonus();
    }

    /// <summary>
    /// Gọi thủ công nếu gold tăng từ nguồn khác (chest, shop...).
    /// Có thể gọi từ nơi khác khi player nhận gold.
    /// </summary>
    public void CheckGoldBonus()
    {
        var stats = PlayerStatManager.Instance;
        var inv = InventoryManager.Instance;
        if (stats == null || inv == null) return;

        int itemCount = inv.GetItemCount(this);
        if (itemCount <= 0) return;

        // Gold mới nhận được kể từ khi equip item
        float currentTotal = inv.GetTotalCoins();
        float goldSinceEquip = Mathf.Max(0f, currentTotal - goldAtEquip);

        // Tính tier mới (mỗi 10 gold = 1 tier)
        float newTierCount = Mathf.Floor(goldSinceEquip / 10f);
        float oldTierCount = Mathf.Floor(goldCountedSoFar / 10f);

        float newTiers = newTierCount - oldTierCount;
        if (newTiers <= 0f) return;

        // Cập nhật gold đã đếm
        goldCountedSoFar = goldSinceEquip;

        // Cộng bonus damage
        float bonus = newTiers * damagePercentPer10Gold * itemCount;
        stats.ModifyDamage(addFlat: bonus);
        totalAppliedBonus += bonus;

        Debug.Log($"<color=yellow>[ItemDamagePerGold]</color> +{bonus:F2}% damage " +
                  $"({newTiers} tier mới, total gold since equip: {goldSinceEquip:F0}, " +
                  $"total bonus: {totalAppliedBonus:F2}%)");
    }
}