// InventoryManager.cs (updated with Item management: owned as Dictionary for stacking, active as List for equipped types)
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

    public Dictionary<Item, int> ownedItems = new Dictionary<Item, int>();
    public List<Item> activeItems = new List<Item>();

    private float totalCoins = 0f;

    public int maxActiveWeapons = 6;
    public int maxActiveBookBuffs = 3;
    public int maxActiveItem = 2;

    // ---- EVENTS (EventHandler chuẩn) ----
    public event EventHandler OnInventoryChanged;
    public event EventHandler OnActiveWeaponsChanged;
    public event EventHandler OnActiveBookBuffsChanged;
    public event EventHandler OnActiveItemsChanged;

    // Event để WeaponManager biết inventory đã sẵn sàng
    public event EventHandler OnInventoryReady;

    private bool readyInvoked = false;
    public bool IsReady { get; private set; } = false;

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
                Debug.Log("<color=red>Inventory: NO DEFAULT WEAPON → thêm vũ khí mặc định");

                if (defaultWeapon != null)
                {
                    Weapon runtimeDefault = Instantiate(defaultWeapon);
                    // AddWeapon(runtimeDefault);
                    ownedWeapons.Add(runtimeDefault);
                    activeWeapons.Add(runtimeDefault);
                }
            }
            else if (activeWeapons.Count == 0)
            {
                activeWeapons.Add(ownedWeapons[0]);
            }

            // 🔥 ĐÁNH DẤU READY TRƯỚC
            IsReady = true;
            Debug.Log("InventoryManager READY");
            OnInventoryReady?.Invoke(this, EventArgs.Empty);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            var item = ItemManager.Instance.GetRandomItem();
            AddItem(item);   
            Debug.Log($"<color=green> Thêm {item}");
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

    public bool HasBookBuff(BookBuff bb)
    {
        return ownedBookBuffs.Any(b => b.buffId == bb.buffId);
    }

    public void RaiseInventoryChanged()
    {
        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    // --------- ITEM OPERATIONS ---------
    public void AddItem(Item runtimeItem, int count = 1, bool autoEquip = true)
    {
        if (runtimeItem == null || count <= 0) return;

        bool wasOwned = ownedItems.ContainsKey(runtimeItem);
        int previousCount = wasOwned ? ownedItems[runtimeItem] : 0;

        if (!runtimeItem.isStackable && wasOwned) return; // Cannot add if not stackable and already owned

        ownedItems[runtimeItem] = previousCount + count;

        bool isActive = activeItems.Contains(runtimeItem);

        if (autoEquip && !isActive && activeItems.Count < maxActiveItem)
        {
            activeItems.Add(runtimeItem);
            ItemManager.Instance.ApplyItemEffect(runtimeItem, ownedItems[runtimeItem]); // Apply full stack
            OnActiveItemsChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (isActive)
        {
            ItemManager.Instance.ApplyItemEffect(runtimeItem, count); // Apply added count
        }

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItem(Item item, int count = 1)
    {
        if (item == null || count <= 0 || !ownedItems.ContainsKey(item)) return;

        int previousCount = ownedItems[item];

        if (activeItems.Contains(item))
        {
            ItemManager.Instance.RemoveItemEffect(item, count);
        }

        ownedItems[item] -= count;
        if (ownedItems[item] <= 0)
        {
            ownedItems.Remove(item);
            if (activeItems.Remove(item))
            {
                OnActiveItemsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void EquipItem(Item item)
    {
        if (item == null || activeItems.Contains(item) || activeItems.Count >= maxActiveItem || !ownedItems.ContainsKey(item)) return;

        activeItems.Add(item);
        ItemManager.Instance.ApplyItemEffect(item, ownedItems[item]);
        OnActiveItemsChanged?.Invoke(this, EventArgs.Empty);
        OnInventoryChanged?.Invoke(this, EventArgs.Empty); // To refresh UI visuals
    }

    public void UnequipItem(Item item)
    {
        if (item == null || !activeItems.Remove(item)) return;

        ItemManager.Instance.RemoveAllForItem(item);
        OnActiveItemsChanged?.Invoke(this, EventArgs.Empty);
        OnInventoryChanged?.Invoke(this, EventArgs.Empty); // To refresh UI visuals
    }

    public bool HasItem(Item item)
    {
        return ownedItems.ContainsKey(item) && ownedItems[item] > 0;
    }

    // --------- COINS OPERATIONS ---------
    public void AddCoins()
    {
        totalCoins += PlayerStatManager.Instance.Coin;
    }
    public void SpendCoins(float amount)
    {
        if (amount <= 0 || amount > totalCoins) return;
        totalCoins -= amount;
    }
    public float GetTotalCoins()
    {
        return totalCoins;
    }

    private void OnEnable()
    {
        GameEventManager.Instance.OnChestInteracted += HandleChestInteracted;
        Debug.Log("<color= magenta>[InventoryManager]</color> Đăng ký lắng nghe sự kiện OnChestInteracted");

        EnemyManager.Instance.OnDead += EnemyManager_OnDead;
    }
    private void OnDisable()
    {
        GameEventManager.Instance.OnChestInteracted -= HandleChestInteracted;
        Debug.Log("<color= magenta>[InventoryManager]</color> Hủy đăng ký lắng nghe sự kiện OnChestInteracted");

        EnemyManager.Instance.OnDead -= EnemyManager_OnDead;
    }
    private void HandleChestInteracted(object sender, GameEventManager.OnChestInteractEventArgs e)
    {
        AddItem(e.item);
        Debug.Log($"<color= magenta>[InventoryManager]</color> Nhận vật phẩm từ Chest: {e.item}");
    }
    private void EnemyManager_OnDead(object sender, EnemyManager.OnEnemyDeathEventArgs e)
    {
        AddCoins();
    }
}