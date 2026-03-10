// InventorySlotUI.cs (updated to support Weapon, BookBuff, and Item with stacking)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public enum InventoryItemType { Weapon, BookBuff, Item }

/// <summary>
/// Unified slot UI that can represent Weapon, BookBuff, or Item.
/// - Shows icon
/// - Shows equipped overlay if equipped
/// - Shows count for stackable items
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image icon;
    public GameObject equippedOverlay;      // Indicator cho equipped items
    public GameObject highlightSelected;    // Optional for gamepad/selection
    public TMP_Text countText;              // Hiển thị số lượng stack

    [Header("Item Data")]
    private InventoryItemType itemType;
    private Weapon weaponData;
    private BookBuff bookBuffData;
    private Item itemData;
    private int itemCount;

    /// <summary>
    /// Bind cho Item với số lượng stack
    /// </summary>
    public void Bind(Item data, int count)
    {
        itemType = InventoryItemType.Item;
        itemData = data;
        itemCount = count;
        weaponData = null;
        bookBuffData = null;

        // Set icon
        if (icon != null)
        {
            icon.sprite = data != null ? data.Icon : null;
            icon.enabled = data != null && data.Icon != null;
        }

        // Set count text
        if (countText != null)
        {
            if (count > 1)
            {
                countText.text = $"x{count}";
                countText.gameObject.SetActive(true);
            }
            else
            {
                countText.text = "";
                countText.gameObject.SetActive(false);
            }
        }

        UpdateEquippedVisual();
    }

    /// <summary>
    /// Bind cho Weapon
    /// </summary>
    public void Bind(Weapon data)
    {
        itemType = InventoryItemType.Weapon;
        weaponData = data;
        bookBuffData = null;
        itemData = null;
        itemCount = 0;

        // Set icon
        if (icon != null)
        {
            icon.sprite = data != null ? data.Icon : null;
            icon.enabled = data != null && data.Icon != null;
        }

        // Hide count for weapons
        if (countText != null)
        {
            countText.gameObject.SetActive(false);
        }

        UpdateEquippedVisual();
    }

    /// <summary>
    /// Bind cho BookBuff
    /// </summary>
    public void Bind(BookBuff data)
    {
        itemType = InventoryItemType.BookBuff;
        bookBuffData = data;
        weaponData = null;
        itemData = null;
        itemCount = 0;

        // Set icon
        if (icon != null)
        {
            icon.sprite = data != null ? data.Icon : null;
            icon.enabled = data != null && data.Icon != null;
        }

        // Hide count for buffs
        if (countText != null)
        {
            countText.gameObject.SetActive(false);
        }

        UpdateEquippedVisual();
    }

    /// <summary>
    /// Update visual để hiển thị equipped state
    /// </summary>
    public void UpdateEquippedVisual()
    {
        if (equippedOverlay == null) return;
        if (InventoryManager.Instance == null)
        {
            equippedOverlay.SetActive(false);
            return;
        }

        bool isEquipped = false;

        if (itemType == InventoryItemType.Weapon && weaponData != null)
        {
            isEquipped = InventoryManager.Instance.activeWeapons.Contains(weaponData);
        }
        else if (itemType == InventoryItemType.BookBuff && bookBuffData != null)
        {
            isEquipped = InventoryManager.Instance.activeBookBuffs.Contains(bookBuffData);
        }
        else if (itemType == InventoryItemType.Item && itemData != null)
        {
            isEquipped = InventoryManager.Instance.activeItems.Contains(itemData);
        }

        equippedOverlay.SetActive(isEquipped);
    }

    /// <summary>
    /// Optional: Set highlight state
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (highlightSelected != null)
        {
            highlightSelected.SetActive(selected);
        }
    }
}