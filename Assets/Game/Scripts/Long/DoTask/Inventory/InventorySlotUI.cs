// InventorySlotUI.cs (updated to support both Weapon and BookBuff)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public enum InventoryItemType { Weapon, BookBuff }

/// <summary>
/// Unified slot UI that can represent Weapon or BookBuff.
/// - Shows icon
/// - Shows equipped overlay if equipped
/// - On pointer hover/show can call InventoryTooltip.Instance.Show(...)
/// Attach this script to your slot prefab.
/// </summary>
public class InventorySlotUI : MonoBehaviour
// IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image icon;
    public GameObject equippedOverlay; // small dot/outline to indicate equipped (optional)
    public GameObject highlightSelected; // optional for gamepad/selection feedback

    InventoryItemType itemType;
    Weapon weaponData;
    BookBuff bookBuffData;

    public void Bind(Weapon data)
    {
        itemType = InventoryItemType.Weapon;
        weaponData = data;
        bookBuffData = null;

        if (icon != null)
        {
            icon.sprite = data != null ? data.Icon : null;
            icon.enabled = data != null && data.Icon != null;
        }

        UpdateEquippedVisual();
    }

    public void Bind(BookBuff data)
    {
        itemType = InventoryItemType.BookBuff;
        bookBuffData = data;
        weaponData = null;

        if (icon != null)
        {
            icon.sprite = data != null ? data.Icon : null; // Assuming BookBuff has Icon field; add if missing
            icon.enabled = data != null && data.Icon != null;
        }

        UpdateEquippedVisual();
    }

    public void UpdateEquippedVisual()
    {
        if (equippedOverlay == null) return;
        if (InventoryManager.Instance == null)
        {
            equippedOverlay.SetActive(false);
            return;
        }

        bool eq = false;
        if (itemType == InventoryItemType.Weapon && weaponData != null)
        {
            eq = InventoryManager.Instance.activeWeapons.Contains(weaponData);
        }
        else if (itemType == InventoryItemType.BookBuff && bookBuffData != null)
        {
            eq = InventoryManager.Instance.activeBookBuffs.Contains(bookBuffData);
        }

        equippedOverlay.SetActive(eq);
    }

    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     if (InventoryTooltip.Instance != null)
    //     {
    //         if (itemType == InventoryItemType.Weapon && weaponData != null)
    //             InventoryTooltip.Instance.Show(weaponData, Input.mousePosition);
    //         else if (itemType == InventoryItemType.BookBuff && bookBuffData != null)
    //             InventoryTooltip.Instance.Show(bookBuffData, Input.mousePosition); // Assuming overload for BookBuff
    //     }
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     if (InventoryTooltip.Instance != null) InventoryTooltip.Instance.Hide();
    // }

    // public void OnPointerClick(PointerEventData eventData)
    // {
    //     if (InventoryManager.Instance == null) return;

    //     // Left click: toggle equip
    //     if (itemType == InventoryItemType.Weapon && weaponData != null)
    //     {
    //         if (InventoryManager.Instance.activeWeapons.Contains(weaponData))
    //             InventoryManager.Instance.UnequipWeapon(weaponData);
    //         else
    //             InventoryManager.Instance.EquipWeapon(weaponData);
    //     }
    //     else if (itemType == InventoryItemType.BookBuff && bookBuffData != null)
    //     {
    //         if (InventoryManager.Instance.activeBookBuffs.Contains(bookBuffData))
    //             InventoryManager.Instance.UnequipBookBuff(bookBuffData);
    //         else
    //             InventoryManager.Instance.EquipBookBuff(bookBuffData);
    //     }

    //     UpdateEquippedVisual();
    // }

    // // Optional: allow external code to highlight/select this slot
    // public void SetSelected(bool sel)
    // {
    //     if (highlightSelected != null) highlightSelected.SetActive(sel);
    // }
}