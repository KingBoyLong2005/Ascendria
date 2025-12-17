using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpOptionUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text title;
    public Button button;
    public Image placeholder;

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

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(option));
    }
}
