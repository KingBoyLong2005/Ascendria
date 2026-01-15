using UnityEngine;

public static class WeaponUpgrade
{
    public static void Upgrade(Weapon weapon, Rarity rarity)
    {   
        // if (!Application.isPlaying || weapon == null)
        //     return;
        weapon.LevelUp(rarity);
        Debug.Log($"{weapon.weaponName} upgraded to Lv.{weapon.level} ({rarity})");
    }
}
public static class RarityHelper
{
    // Base weight (tổng = 1.0)
    static readonly float[] baseWeights =
    {
        0.40f, // Common
        0.30f, // Uncommon
        0.15f, // Rare
        0.10f, // Epic
        0.05f  // Legendary
    };

    /// <summary>
    /// Roll rarity dựa trên Luck (0 → 1)
    /// Luck làm dịch xác suất từ tier thấp lên tier cao
    /// </summary>
    public static Rarity GetRandomRarity(float luck = 0f)
    {
        luck = Mathf.Clamp01(luck);

        // Clone để không phá baseWeights
        float[] weights = (float[])baseWeights.Clone();

        // Luck bias: đẩy xác suất lên tier cao hơn
        for (int i = 0; i < weights.Length - 1; i++)
        {
            float shift = weights[i] * luck * 0.6f;

            weights[i]     -= shift;
            weights[i + 1] += shift;
        }

        return RollByWeight(weights);
    }

    // ================= INTERNAL =================
    static Rarity RollByWeight(float[] weights)
    {
        float sum = 0f;
        foreach (var w in weights) sum += w;

        float r = Random.value * sum;

        for (int i = 0; i < weights.Length; i++)
        {
            if (r < weights[i])
                return (Rarity)i;

            r -= weights[i];
        }

        return Rarity.Common;
    }
}