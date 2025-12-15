// InventoryManager.cs (added Equip/Unequip for BookBuff, consistent with weapons)
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // Danh sách toàn bộ vũ khí player sở hữu
    public List<Weapon> ownedWeapons = new List<Weapon>();

    // Danh sách vũ khí đang trang bị (equipped)
    public List<Weapon> activeWeapons = new List<Weapon>();

    public List<BookBuff> ownedBookBuffs = new List<BookBuff>();
    public List<BookBuff> activeBookBuffs = new List<BookBuff>();

    public int maxActiveWeapons = 6;
    public int maxActiveBookBuffs = 3;

    // ---- EVENTS (EventHandler chuẩn) ----
    public event EventHandler OnInventoryChanged;
    public event EventHandler OnActiveWeaponsChanged;
    public event EventHandler OnActiveBookBuffsChanged;

    // Event để WeaponManager biết inventory đã sẵn sàng
    public event EventHandler OnInventoryReady;

    private bool readyInvoked = false;

    // public Weapon defaultWeapon;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (!readyInvoked)
        {
            readyInvoked = true;

            // 🔥 ĐẶT WEAPON MẶC ĐỊNH TẠI ĐÂY
            if (ownedWeapons.Count == 0)
            {
                var defaultWeapon = FindFirstObjectByType<PlayerAttack>().wp;
                Debug.Log("Inventory: NO DEFAULT WEAPON → thêm vũ khí mặc định");

                if (defaultWeapon != null)
                {
                    Weapon runtimeDefault = Instantiate(defaultWeapon);
                    ownedWeapons.Add(runtimeDefault);
                    activeWeapons.Add(runtimeDefault);
                }
            }
            else if (activeWeapons.Count == 0)
            {
                activeWeapons.Add(ownedWeapons[0]);
            }

            Debug.Log("InventoryManager READY");
            OnInventoryReady?.Invoke(this, EventArgs.Empty);
        }
    }


    // --------- WEAPON OPERATIONS ---------
    public void AddWeapon(Weapon runtimeWeapon, bool autoEquip = true)
    {
        if (runtimeWeapon == null) return;

        ownedWeapons.Add(runtimeWeapon);

        if (autoEquip && activeWeapons.Count < maxActiveWeapons)
            activeWeapons.Add(runtimeWeapon);

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
        OnActiveWeaponsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveWeapon(Weapon weapon)
    {
        if (weapon == null) return;

        ownedWeapons.Remove(weapon);
        activeWeapons.Remove(weapon);

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
        OnActiveWeaponsChanged?.Invoke(this, EventArgs.Empty);
    }

    // public void EquipWeapon(Weapon weapon)
    // {
    //     if (weapon == null) return;
    //     if (activeWeapons.Contains(weapon)) return;
    //     if (activeWeapons.Count >= maxActiveWeapons) return;

    //     activeWeapons.Add(weapon);
    //     OnActiveWeaponsChanged?.Invoke(this, EventArgs.Empty);
    // }

    // public void UnequipWeapon(Weapon weapon)
    // {
    //     if (weapon == null) return;

    //     if (activeWeapons.Remove(weapon))
    //         OnActiveWeaponsChanged?.Invoke(this, EventArgs.Empty);
    // }

    // tiện ích
    public bool HasWeapon(Weapon w) => ownedWeapons.Contains(w);

    // --------- Buff OPERATIONS ---------
    public void AddBuff(BookBuff runtimeBookBuff, bool autoEquip = true)
    {
        if(runtimeBookBuff == null) return;
        
        if (HasBookBuff(runtimeBookBuff)) return;
        ownedBookBuffs.Add(runtimeBookBuff);

        if(autoEquip && activeBookBuffs.Count < maxActiveBookBuffs)
            activeBookBuffs.Add(runtimeBookBuff);

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
        OnActiveBookBuffsChanged?.Invoke(this, EventArgs.Empty);
    }
    public void RemoveBookBuff(BookBuff bookBuff)
    {
        if (bookBuff == null) return;

        ownedBookBuffs.Remove(bookBuff);
        activeBookBuffs.Remove(bookBuff);

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
        OnActiveBookBuffsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void EquipBookBuff(BookBuff bookBuff)
    {
        if (bookBuff == null) return;
        if (activeBookBuffs.Contains(bookBuff)) return;
        if (activeBookBuffs.Count >= maxActiveBookBuffs) return;

        activeBookBuffs.Add(bookBuff);
        OnActiveBookBuffsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UnequipBookBuff(BookBuff bookBuff)
    {
        if (bookBuff == null) return;

        if (activeBookBuffs.Remove(bookBuff))
            OnActiveBookBuffsChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HasBookBuff(BookBuff bb)
    {
        return ownedBookBuffs.Any(b => b.buffId == bb.buffId);
    }
    // New: Sort methods (e.g., by name alphabetically; customize as needed)
    private void SortOwnedWeapons()
    {
        ownedWeapons = ownedWeapons.OrderBy(w => w.weaponName).ToList(); // Sort by weaponName
    }

    private void SortOwnedBookBuffs()
    {
        ownedBookBuffs = ownedBookBuffs.OrderBy(b => b.name).ToList(); // Sort by name
    }
}