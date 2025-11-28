using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// LevelUpOptionUI (cập nhật)
/// - Setup(UpgradeOption, Action<UpgradeOption>) sẽ hiển thị icon dựa trên UpgradeOption.targetWeapon nếu có.
/// - Nếu buff và không có icon, sẽ cố gắng lấy placeholder (có thể set từ inspector).
/// </summary>
public class LevelUpOptionUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text titleText;
    public Button button;

    // optional placeholder icon to use for buff when no sprite exists
    public Sprite placeholderIcon;

    LevelUpUI.UpgradeOption upgrade;
    Action<LevelUpUI.UpgradeOption> onUpgradeSelected;

    void Awake()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }

    // Setup for UpgradeOption (weapon upgrade/drop or buff)
    public void Setup(LevelUpUI.UpgradeOption up, Action<LevelUpUI.UpgradeOption> callback)
    {
        upgrade = up;
        onUpgradeSelected = callback;

        // Title text based on kind
        switch (up.kind)
        {
            case LevelUpUI.UpgradeOption.Kind.WeaponUpgrade:
                if (up.targetWeapon != null)
                    titleText.text = $"{up.tier} Upgrade: {up.targetWeapon.displayName}";
                else
                    titleText.text = $"{up.tier} Weapon Upgrade";
                break;
            case LevelUpUI.UpgradeOption.Kind.WeaponDrop:
                if (up.targetWeapon != null)
                    titleText.text = $"{up.tier} Drop: {up.targetWeapon.displayName}";
                else
                    titleText.text = $"{up.tier} Weapon Drop";
                break;
            case LevelUpUI.UpgradeOption.Kind.Buff:
                titleText.text = $"{up.tier} {up.buffType}";
                break;
        }

        // Set icon:
        Sprite s = null;
        if (up.targetWeapon != null && up.targetWeapon.icon != null)
        {
            s = up.targetWeapon.icon;
        }
        else
        {
            // For buff: try placeholder or null
            s = placeholderIcon;
        }

        if (icon != null)
        {
            icon.sprite = s;
            icon.enabled = (s != null);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onUpgradeSelected?.Invoke(upgrade));
    }

    // Optional: if you sometimes call Setup for WeaponData directly
    public void Setup(WeaponData w, Action<WeaponData> callback)
    {
        titleText.text = w != null ? w.displayName : "Weapon";
        if (icon != null)
        {
            icon.sprite = (w != null) ? w.icon : placeholderIcon;
            icon.enabled = (icon.sprite != null);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback?.Invoke(w));
    }
}
