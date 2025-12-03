using UnityEngine;

public static class WeaponUpgrade
{
    public static void Upgrade(Weapon weapon, Rarity rarity)
    {
        weapon.LevelUp(rarity);
        Debug.Log($"{weapon.weaponName} upgraded to Lv.{weapon.level} ({rarity})");
    }
}
public static class RarityHelper
{
    public static Rarity GetRandomRarity()
    {
        float r = Random.value; // 0 → 1

        // tỉ lệ mặc định (bạn có thể chỉnh)
        if (r < 0.50f) return Rarity.Common;      // 50%
        if (r < 0.75f) return Rarity.Uncommon;    // 25%
        if (r < 0.90f) return Rarity.Rare;        // 15%
        if (r < 0.98f) return Rarity.Epic;        // 8%
        return Rarity.Legendary;                  // 2%
    }
}
