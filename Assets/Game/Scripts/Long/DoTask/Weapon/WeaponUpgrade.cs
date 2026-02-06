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

    public static Rarity GetRandomRarity(float luck = 0f)
    {
        luck = Mathf.Clamp01(luck);

        float[] w = (float[])baseWeights.Clone();

        // 1. Rút xác suất từ tier thấp
        float lowPool =
            w[0] * luck * 0.6f +   // Common
            w[1] * luck * 0.4f;    // Uncommon

        w[0] -= w[0] * luck * 0.6f;
        w[1] -= w[1] * luck * 0.4f;

        // 2. Phân phối cho Rare & Epic
        w[2] += lowPool * 0.55f; // Rare
        w[3] += lowPool * 0.35f; // Epic

        // 3. Legendary: bonus nhỏ + soft cap
        float legendaryBonus = Mathf.Min(0.03f * luck, 0.03f); // max +3%
        w[4] += legendaryBonus;

        // 4. Trừ phần legendary bonus từ Rare/Epic
        float deduct = legendaryBonus;
        w[2] -= deduct * 0.6f;
        w[3] -= deduct * 0.4f;

        Normalize(w);
        return RollByWeight(w);
    }
    static void Normalize(float[] w)
    {
        float sum = 0f;
        for (int i = 0; i < w.Length; i++)
        {
            w[i] = Mathf.Max(0f, w[i]);
            sum += w[i];
        }

        for (int i = 0; i < w.Length; i++)
            w[i] /= sum;
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