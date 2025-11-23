using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // chứa các item chung (consumable, passive, hoặc bất kỳ InventoryItemBase nào bạn muốn lưu)
    public List<InventoryItemBase> items = new List<InventoryItemBase>();

    // vũ khí giữ riêng do logic combat
    public List<WeaponData> ownedWeapons = new List<WeaponData>();
    public List<WeaponData> activeWeapons = new List<WeaponData>(); // equipped / sử dụng trong trận

    public int maxActiveWeapons = 6;
    public event Action OnChanged;
    public event Action OnWeaponsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------- ITEMS ----------
    public void AddItem(InventoryItemBase newItem)
    {
        if (newItem == null) return;
        items.Add(newItem);
        OnChanged?.Invoke();
    }

    public void RemoveItem(InventoryItemBase item)
    {
        if (item == null) return;
        if (items.Remove(item)) OnChanged?.Invoke();
    }

    // helper overloads for backward compatibility (if other code calls AddItem(ItemData))
    public void AddItem(ItemData newItem) => AddItem((InventoryItemBase)newItem);
    public void RemoveItem(ItemData item) => RemoveItem((InventoryItemBase)item);

    // ---------- WEAPONS ----------
    public void AddWeapon(WeaponData w, bool equipIfSpace = true)
    {
        if (w == null) return;
        if (!ownedWeapons.Contains(w))
            ownedWeapons.Add(w);
        if (equipIfSpace && activeWeapons.Count < maxActiveWeapons && !activeWeapons.Contains(w))
            activeWeapons.Add(w);
        OnWeaponsChanged?.Invoke();
        OnChanged?.Invoke(); // optional: items changed visually
    }

    public void RemoveWeapon(WeaponData w)
    {
        if (w == null) return;
        ownedWeapons.Remove(w);
        activeWeapons.Remove(w);
        OnWeaponsChanged?.Invoke();
        OnChanged?.Invoke();
    }

    public void EquipWeapon(WeaponData w)
    {
        if (w == null) return;
        if (activeWeapons.Contains(w)) return;
        if (activeWeapons.Count >= maxActiveWeapons) return;
        activeWeapons.Add(w);
        OnWeaponsChanged?.Invoke();
    }

    public void UnequipWeapon(WeaponData w)
    {
        if (w == null) return;
        if (activeWeapons.Remove(w)) OnWeaponsChanged?.Invoke();
    }

    // Convenience queries
    public bool HasItem(InventoryItemBase item) => items.Contains(item);
    public bool HasWeapon(WeaponData w) => ownedWeapons.Contains(w);
}
