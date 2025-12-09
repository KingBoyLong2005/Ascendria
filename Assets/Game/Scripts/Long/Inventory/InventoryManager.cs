using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // Danh sách toàn bộ vũ khí player sở hữu
    public List<Weapon> ownedWeapons = new List<Weapon>();

    // Danh sách vũ khí đang trang bị (equipped)
    public List<Weapon> activeWeapons = new List<Weapon>();

    public int maxActiveWeapons = 6;

    // ---- EVENTS (EventHandler chuẩn) ----
    public event EventHandler OnInventoryChanged;
    public event EventHandler OnActiveWeaponsChanged;

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

                // GIẢ SỬ BẠN GÁN TRONG INSPECTOR một weapon default
                // như wpDefault hoặc weapon nào bạn muốn
                if (defaultWeapon != null)
                {
                    ownedWeapons.Add(defaultWeapon);
                    activeWeapons.Add(defaultWeapon);
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
    public void AddWeapon(Weapon weapon, bool autoEquip = true)
    {
        if (weapon == null) return;

        if (!ownedWeapons.Contains(weapon))
            ownedWeapons.Add(weapon);

        if (autoEquip && activeWeapons.Count < maxActiveWeapons && !activeWeapons.Contains(weapon))
            activeWeapons.Add(weapon);

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

    public void EquipWeapon(Weapon weapon)
    {
        if (weapon == null) return;
        if (activeWeapons.Contains(weapon)) return;
        if (activeWeapons.Count >= maxActiveWeapons) return;

        activeWeapons.Add(weapon);
        OnActiveWeaponsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UnequipWeapon(Weapon weapon)
    {
        if (weapon == null) return;

        if (activeWeapons.Remove(weapon))
            OnActiveWeaponsChanged?.Invoke(this, EventArgs.Empty);
    }

    // tiện ích
    public bool HasWeapon(Weapon w) => ownedWeapons.Contains(w);
}
