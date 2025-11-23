using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class WeaponSlotUI : MonoBehaviour{
    public Image icon;
    public Image equippedOverlay; // optional: small dot/outline to mark equipped
    WeaponData weapon;

    public void Bind(WeaponData w)
    {
        weapon = w;
        if (icon != null)
            icon.sprite = w != null ? w.icon : null;
        if (icon != null)
            icon.enabled = w != null && w.icon != null;

        UpdateEquippedVisual();
    }

    public void UpdateEquippedVisual()
    {
        if (equippedOverlay == null || InventoryManager.Instance == null)
            return;
        bool eq = weapon != null && InventoryManager.Instance.activeWeapons.Contains(weapon);
        equippedOverlay.gameObject.SetActive(eq);
    }
}
