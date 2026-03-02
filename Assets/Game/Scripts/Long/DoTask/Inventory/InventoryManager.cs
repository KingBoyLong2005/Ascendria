using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Weapons")]
    public List<Weapon> ownedWeapons = new List<Weapon>();
    public List<Weapon> activeWeapons = new List<Weapon>();
    public int maxActiveWeapons = 3;

    [Header("Book Buffs")]
    public List<BookBuff> ownedBookBuffs = new List<BookBuff>();
    public List<BookBuff> activeBookBuffs = new List<BookBuff>();
    public int maxActiveBookBuffs = 3;

    [Header("Items")]
    public Dictionary<Item, int> ownedItems = new Dictionary<Item, int>();
    public List<Item> activeItems = new List<Item>();
    // Bỏ giới hạn maxActiveItem - tất cả items đều auto-active

    [Header("Coins")]
    private float totalCoins = 0f;
    [Header("Coins")]

    private float totalSilver = 0f;

    // ---- EVENTS ----
    public event EventHandler OnInventoryChanged;
    public event EventHandler OnActiveWeaponsChanged;
    public event EventHandler OnActiveBookBuffsChanged;
    public event EventHandler OnActiveItemsChanged;
    public event EventHandler OnInventoryReady;

    public event EventHandler<SilverProgressEventArgs> OnSilverChanged;
    public Action OnSilverChange;

    private bool readyInvoked = false;
    public bool IsReady { get; private set; } = false;
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

            // Thêm weapon mặc định
            if (ownedWeapons.Count == 0)
            {
                var defaultWeapon = FindFirstObjectByType<ProfileCharacterLoader>()?.profile?.startingWeapon;
                if (defaultWeapon != null)
                {
                    Weapon runtimeDefault = Instantiate(defaultWeapon);
                    ownedWeapons.Add(runtimeDefault);
                    activeWeapons.Add(runtimeDefault);
                    Debug.Log("<color=green>Inventory: Added default weapon</color>");
                }
            }
            else if (activeWeapons.Count == 0)
            {
                activeWeapons.Add(ownedWeapons[0]);
            }

            IsReady = true;
            FindFirstObjectByType<InventoryUI>().RefreshUIScene();
            Debug.Log("<color=green>InventoryManager READY</color>");
            OnInventoryReady?.Invoke(this, EventArgs.Empty);
        }
    }

    void Update()
    {
        // Test key - remove in production
        if (Input.GetKeyDown(KeyCode.M))
        {
            var item = ItemManager.Instance?.GetRandomItem();
            if (item != null)
            {
                AddItem(item);
                Debug.Log($"<color=green>Added {item.name}</color>");
            }
        }
    }

    #region WEAPON OPERATIONS 
    
    public void AddWeapon(Weapon runtimeWeapon, bool autoEquip = true)
    {
        if (runtimeWeapon == null) return;

        Weapon existingWeapon = ownedWeapons.FirstOrDefault(w => w.weaponName == runtimeWeapon.weaponName);
        
        if (existingWeapon != null)
        {
            Debug.Log($"<color=yellow>[InventoryManager] Weapon '{runtimeWeapon.weaponName}' already exists</color>");
            return;
        }

        ownedWeapons.Add(runtimeWeapon);
        Debug.Log($"<color=green>[InventoryManager] Added weapon: {runtimeWeapon.weaponName}</color>");

        if (autoEquip && activeWeapons.Count < maxActiveWeapons)
        {
            activeWeapons.Add(runtimeWeapon);
            OnActiveWeaponsChanged?.Invoke(this, EventArgs.Empty);
        }

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveWeapon(Weapon weapon)
    {
        if (weapon == null) return;

        ownedWeapons.Remove(weapon);
        activeWeapons.Remove(weapon);

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
        OnActiveWeaponsChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HasWeapon(Weapon w)
    {
        if (w == null) return false;
        return ownedWeapons.Any(owned => owned.weaponName == w.weaponName);
    }
    
    public Weapon GetWeaponByName(string weaponName)
    {
        return ownedWeapons.FirstOrDefault(w => w.weaponName == weaponName);
    }
    
    #endregion
    
    #region BUFF OPERATIONS 
    
    public void AddBuff(BookBuff runtimeBookBuff, bool autoEquip = true)
    {
        if (runtimeBookBuff == null) return;
        
        if (HasBookBuff(runtimeBookBuff))
        {
            Debug.Log($"<color=yellow>[InventoryManager] Buff '{runtimeBookBuff.buffId}' already exists</color>");
            return;
        }

        ownedBookBuffs.Add(runtimeBookBuff);
        Debug.Log($"<color=green>[InventoryManager] Added buff: {runtimeBookBuff.buffId}</color>");

        if (autoEquip && activeBookBuffs.Count < maxActiveBookBuffs)
        {
            activeBookBuffs.Add(runtimeBookBuff);
            OnActiveBookBuffsChanged?.Invoke(this, EventArgs.Empty);
        }

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
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
        if (bb == null) return false;
        return ownedBookBuffs.Any(b => b.buffId == bb.buffId);
    }
    
    public BookBuff GetBuffById(string buffId)
    {
        return ownedBookBuffs.FirstOrDefault(b => b.buffId == buffId);
    }

    public void RaiseInventoryChanged()
    {
        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }
    
    #endregion

    #region ITEM OPERATIONS 
    
    /// <summary>
    /// Thêm item vào inventory. Items tự động active (không giới hạn).
    /// </summary>
    public void AddItem(Item runtimeItem, int count = 1, bool autoEquip = true)
    {
        if (runtimeItem == null || count <= 0) return;

        bool wasOwned = ownedItems.ContainsKey(runtimeItem);
        int previousCount = wasOwned ? ownedItems[runtimeItem] : 0;

        // Check stackable
        if (!runtimeItem.isStackable && wasOwned)
        {
            Debug.Log($"<color=yellow>[InventoryManager] Item '{runtimeItem.name}' is not stackable</color>");
            return;
        }

        // Update count
        ownedItems[runtimeItem] = previousCount + count;
        Debug.Log($"<color=green>[InventoryManager] Added {count}x {runtimeItem.name} (total: {ownedItems[runtimeItem]})</color>");

        bool isActive = activeItems.Contains(runtimeItem);

        // Auto-equip logic - BỎ GIỚI HẠN maxActiveItem
        if (autoEquip && !isActive)
        {
            // Tự động active tất cả items
            activeItems.Add(runtimeItem);
            ItemManager.Instance?.ApplyItemEffect(runtimeItem, ownedItems[runtimeItem]);
            OnActiveItemsChanged?.Invoke(this, EventArgs.Empty);
            
            Debug.Log($"<color=cyan>[InventoryManager] Auto-equipped {runtimeItem.name}</color>");
        }
        else if (isActive)
        {
            // Item đã active → chỉ apply thêm count mới
            ItemManager.Instance?.ApplyItemEffect(runtimeItem, count);
        }

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Remove item khỏi inventory
    /// </summary>
    public void RemoveItem(Item item, int count = 1)
    {
        if (item == null || count <= 0 || !ownedItems.ContainsKey(item)) return;

        // Remove effect nếu đang active
        if (activeItems.Contains(item))
        {
            ItemManager.Instance?.RemoveItemEffect(item, count);
        }

        // Update count
        ownedItems[item] -= count;
        Debug.Log($"<color=orange>[InventoryManager] Removed {count}x {item.name} (remaining: {ownedItems[item]})</color>");

        // Nếu hết item → remove khỏi dictionary và active list
        if (ownedItems[item] <= 0)
        {
            ownedItems.Remove(item);
            if (activeItems.Remove(item))
            {
                OnActiveItemsChanged?.Invoke(this, EventArgs.Empty);
                Debug.Log($"<color=red>[InventoryManager] Item {item.name} depleted</color>");
            }
        }

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Check xem có item không
    /// </summary>
    public bool HasItem(Item item)
    {
        return ownedItems.ContainsKey(item) && ownedItems[item] > 0;
    }

    /// <summary>
    /// Lấy số lượng của item
    /// </summary>
    public int GetItemCount(Item item)
    {
        return ownedItems.ContainsKey(item) ? ownedItems[item] : 0;
    }

    #endregion

    #region COINS OPERATIONS
    
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
    
    #endregion

    #region Silver OPERATIONS

    public void AddSilver(float amount)
    {
        totalSilver += PlayerStatManager.Instance.Silver + amount;
        OnSilverChanged?.Invoke(this, new SilverProgressEventArgs(totalSilver));
        OnSilverChange?.Invoke(); // giữ lại Action cũ nếu đang dùng chỗ khác
    }

    public float GetTotalSilver()
    {
        return totalSilver;
    }

    public class SilverProgressEventArgs : EventArgs
    {
        public float currSilver;

        public SilverProgressEventArgs(float currentSilver)
        {
            currSilver = currentSilver;
        }
    }
    #endregion

    private void OnEnable()
    {
        if (GameEventManager.Instance != null)
        {
            //GameEventManager.Instance.OnChestInteracted += HandleChestInteracted;
            Debug.Log("<color=magenta>[InventoryManager]</color> Đăng ký lắng nghe sự kiện OnChestInteracted");
        }
    }
    
    private void OnDisable()
    {   
        if (GameEventManager.Instance != null)
        {
            //GameEventManager.Instance.OnChestInteracted -= HandleChestInteracted;
            Debug.Log("<color=magenta>[InventoryManager]</color> Hủy đăng ký lắng nghe sự kiện OnChestInteracted");
        }
    }
    
    //private void HandleChestInteracted(object sender, GameEventManager.OnChestInteractEventArgs e)
    //{
    //    AddItem(e.item);
    //    Debug.Log($"<color=magenta>[InventoryManager]</color> Nhận vật phẩm từ Chest: {e.item}");
    //}
}