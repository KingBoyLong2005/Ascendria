using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// Unified slot UI that can represent WeaponData or generic InventoryItemBase (ItemData).
/// - Shows icon
/// - Shows equipped overlay when the bound item is a WeaponData and it's in InventoryManager.Instance.activeWeapons
/// - On pointer hover/show can call InventoryTooltip.Instance.Show(...)
/// Attach this script to your slot prefab (replace old WeaponSlotUI / ItemSlotUI prefabs).
/// </summary>
public class InventorySlotUI : MonoBehaviour
// IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image icon;
    public GameObject equippedOverlay; // small dot/outline to indicate equipped (optional)
    public GameObject highlightSelected; // optional for gamepad/selection feedback

    // InventoryItemBase itemData;
    Weapon weaponData;

    public void Bind(Weapon data)
    {
        // itemData = data;
        weaponData = data as Weapon;

        if (icon != null)
        {
            icon.sprite = data != null ? data.Icon : null;
            icon.enabled = data != null && data.Icon != null;
        }

        UpdateEquippedVisual();
    }

    public void UpdateEquippedVisual()
    {
        if (equippedOverlay == null) return;
        if (weaponData == null || InventoryManager.Instance == null)
        {
            equippedOverlay.SetActive(false);
            return;
        }
        bool eq = InventoryManager.Instance.activeWeapons.Contains(weaponData);
        equippedOverlay.SetActive(eq);
    }

    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     if (InventoryTooltip.Instance != null && itemData != null)
    //     {
    //         InventoryTooltip.Instance.Show(itemData, Input.mousePosition);
    //     }
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     if (InventoryTooltip.Instance != null) InventoryTooltip.Instance.Hide();
    // }

    // public void OnPointerClick(PointerEventData eventData)
    // {
    //     // Left click: toggle equip if it's a weapon
    //     if (weaponData != null && InventoryManager.Instance != null)
    //     {
    //         if (InventoryManager.Instance.activeWeapons.Contains(weaponData))
    //             InventoryManager.Instance.UnequipWeapon(weaponData);
    //         else
    //             InventoryManager.Instance.EquipWeapon(weaponData);
    //         UpdateEquippedVisual();
    //     }

    //     // Right click: optional: use consumable / inspect (implement as needed)
    // }

    // // Optional: allow external code to highlight/select this slot
    // public void SetSelected(bool sel)
    // {
    //     if (highlightSelected != null) highlightSelected.SetActive(sel);
    // }
}
