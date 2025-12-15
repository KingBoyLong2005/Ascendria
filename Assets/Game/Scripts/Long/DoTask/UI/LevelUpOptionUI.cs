// LevelUpOptionUI.cs (simplified; now uses EventHandler instead of Action)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LevelUpOptionUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text titleText;
    public Button button;

    // optional placeholder icon to use for buff when no sprite exists
    public Sprite placeholderIcon;

    LevelManager.UpgradeOption upgrade;

    // Event to notify selection (replaces Action)
    public event EventHandler<LevelUpOptionSelectedEventArgs> OnOptionSelected;

    void Awake()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }

    // Setup for UpgradeOption (weapon upgrade/drop or buff)
    public void Setup(LevelManager.UpgradeOption up)
    {
        upgrade = up;

        // Title text based on kind
        switch (up.kind)
        {
            case LevelManager.UpgradeOption.Kind.WeaponUpgrade:
                if (up.targetWeapon != null)
                    titleText.text = $"{up.tier} Upgrade: {up.targetWeapon.weaponName}";
                else
                    titleText.text = $"{up.tier} Weapon Upgrade";
                break;
            case LevelManager.UpgradeOption.Kind.WeaponDrop:
                if (up.targetWeapon != null)
                    titleText.text = $"{up.tier} Drop: {up.targetWeapon.weaponName}";
                else
                    titleText.text = $"{up.tier} Weapon Drop";
                break;
            case LevelManager.UpgradeOption.Kind.Buff:
                titleText.text = $"{up.tier} {up.targetBuff.name}";
                break;
        }

        // Set icon:
        Sprite s = null;

        // Weapon icon
        if (up.targetWeapon != null)
        {
            s = up.targetWeapon.Icon;
        }
        // Buff icon
        else if (up.targetBuff != null)
        {
            s = up.targetBuff.Icon != null ? up.targetBuff.Icon : placeholderIcon;
        }
        // fallback
        else
        {
            s = placeholderIcon;
        }

        if (icon != null)
        {
            icon.sprite = s;
            icon.enabled = (s != null);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnOptionSelected?.Invoke(this, new LevelUpOptionSelectedEventArgs { SelectedUpgrade = upgrade }));
    }

    public class LevelUpOptionSelectedEventArgs : EventArgs
    {
        public LevelManager.UpgradeOption SelectedUpgrade { get; set; }
    }
}