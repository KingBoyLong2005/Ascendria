using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpOptionUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameWeapon;
    public Button button;
    public Image placeholder;
    public Image ColorRare;
    public TMP_Text title;

    public void Setup(
        LevelManager.UpgradeOption option,
        System.Action<LevelManager.UpgradeOption> onClick)
    {
        // Tên hiển thị
        nameWeapon.text = option.kind switch
        {
            LevelManager.UpgradeOption.Kind.WeaponUpgrade =>
                $"{option.tier} Upgrade {option.targetWeapon.weaponName}",

            LevelManager.UpgradeOption.Kind.WeaponDrop =>
                $"{option.tier} Weapon {option.targetWeapon.weaponName}",

            LevelManager.UpgradeOption.Kind.Buff =>
                $"{option.tier} {option.targetBuff.statTarget}",

            _ => "Upgrade"
        };

        // Icon
        icon.sprite =
            option.targetWeapon?.Icon ??
            option.targetBuff?.Icon ??
            placeholder.sprite;
        
        // Màu theo độ hiếm
        if (ColorRare != null)
        {
            ColorRare.color = GetRarityColor(option.tier);
        }

        // ✨ DESCRIPTION - Hiển thị chính xác stats sẽ upgrade
        if (title != null)
        {
            title.text = GetUpgradeDescription(option);
        }

        // Button callback
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(option));
    }

    /// <summary>
    /// Lấy description chính xác cho upgrade
    /// </summary>
    private string GetUpgradeDescription(LevelManager.UpgradeOption option)
    {
        Rarity rarity = ConvertTierToRarity(option.tier);
        
        switch (option.kind)
        {
            case LevelManager.UpgradeOption.Kind.WeaponUpgrade:
                return GetWeaponUpgradeDescription(option.targetWeapon, rarity);

            case LevelManager.UpgradeOption.Kind.WeaponDrop:
                return $"Unlock new weapon!\n{option.targetWeapon?.weaponName}";

            case LevelManager.UpgradeOption.Kind.Buff:
                return option.targetBuff?.GetUpgradeDescription(rarity) ?? "Apply buff";

            default:
                return "Unknown upgrade";
        }
    }

    /// <summary>
    /// Lấy description cho weapon upgrade (chính xác với seed)
    /// </summary>
    private string GetWeaponUpgradeDescription(Weapon weapon, Rarity rarity)
    {
        if (weapon == null) return "Upgrade weapon";

        // Dùng weapon level làm seed để consistent với actual upgrade
        int seed = weapon.level;
        var upgrades = weapon.GetUpgradePreview(rarity, seed);
        
        // Nếu có preview thì hiển thị chi tiết
        if (upgrades.Count > 0 && upgrades[0] != "No preview available")
        {
            return string.Join("\n", upgrades);
        }

        // Fallback: dùng GetUpgradeDescription (hiển thị possibilities)
        return weapon.GetUpgradeDescription(rarity);
    }

    private Rarity ConvertTierToRarity(LevelManager.RarityTier tier)
    {
        return tier switch
        {
            LevelManager.RarityTier.Common => Rarity.Common,
            LevelManager.RarityTier.Uncommon => Rarity.Uncommon,
            LevelManager.RarityTier.Rare => Rarity.Rare,
            LevelManager.RarityTier.Epic => Rarity.Epic,
            LevelManager.RarityTier.Legendary => Rarity.Legendary,
            _ => Rarity.Common
        };
    }

    private Color GetRarityColor(LevelManager.RarityTier tier)
    {
        return tier switch
        {
            LevelManager.RarityTier.Common => new Color(0f, 1f, 0f),
            LevelManager.RarityTier.Uncommon => new Color(0f, 1f, 1f),
            LevelManager.RarityTier.Rare => new Color(0.58f, 0f, 0.83f),
            LevelManager.RarityTier.Epic => new Color(0.8f, 0f, 0f),
            LevelManager.RarityTier.Legendary => new Color(1f, 0.84f, 0f),
            _ => Color.white
        };
    }
}