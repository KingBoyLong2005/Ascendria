using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpOptionUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text title;
    public Button button;
    public Image placeholder;
    public Image ColorRare;

    public void Setup(
        LevelManager.UpgradeOption option,
        System.Action<LevelManager.UpgradeOption> onClick)
    {
        title.text = option.kind switch
        {
            LevelManager.UpgradeOption.Kind.WeaponUpgrade =>
                $"{option.tier} Upgrade {option.targetWeapon.weaponName}",

            LevelManager.UpgradeOption.Kind.WeaponDrop =>
                $"{option.tier} Weapon {option.targetWeapon.weaponName}",

            LevelManager.UpgradeOption.Kind.Buff =>
                $"{option.tier} {option.targetBuff.name}",

            _ => "Upgrade"
        };

        icon.sprite =
            option.targetWeapon?.Icon ??
            option.targetBuff?.Icon ??
            placeholder.sprite;
        
        // Đổi màu theo độ hiếm
        if (ColorRare != null)
        {
            ColorRare.color = GetRarityColor(option.tier);
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(option));
    }
    private Color GetRarityColor(LevelManager.RarityTier tier)
    {
        return tier switch
        {
            LevelManager.RarityTier.Common => new Color(0f, 1f, 0f),        // Xanh lá (Green)
            LevelManager.RarityTier.Uncommon => new Color(0f, 1f, 1f),      // Xanh cyan (Cyan)
            LevelManager.RarityTier.Rare => new Color(0.58f, 0f, 0.83f),    // Tím (Purple)
            LevelManager.RarityTier.Epic => new Color(0.8f, 0f, 0f),        // Đỏ đậm (Dark Red)
            LevelManager.RarityTier.Legendary => new Color(1f, 0.84f, 0f),  // Gold
            _ => Color.white
        };
    }
}
