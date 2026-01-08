// InventoryManager.cs - Fixed to prevent weapon/buff duplicates
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // Danh sách toàn bộ vũ khí player sở hữu (mỗi weapon chỉ có 1 lần)
    public List<Weapon> ownedWeapons = new List<Weapon>();
    public List<Weapon> activeWeapons = new List<Weapon>();

    public List<BookBuff> ownedBookBuffs = new List<BookBuff>();
    public List<BookBuff> activeBookBuffs = new List<BookBuff>();

    public Dictionary<Item, int> ownedItems = new Dictionary<Item, int>();
    public List<Item> activeItems = new List<Item>();

    private float totalCoins = 0f;

    public int maxActiveWeapons = 6;
    public int maxActiveBookBuffs = 3;
    public int maxActiveItem = 2;

    // ---- EVENTS ----
    public event EventHandler OnInventoryChanged;
    public event EventHandler OnActiveWeaponsChanged;
    public event EventHandler OnActiveBookBuffsChanged;
    public event EventHandler OnActiveItemsChanged;
    public event EventHandler OnInventoryReady;

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
                var defaultWeapon = FindFirstObjectByType<ProfileCharacterLoader>().profile.startingWeapon;
                Debug.Log("<color=red>Inventory: NO DEFAULT WEAPON → thêm vũ khí mặc định</color>");

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

            IsReady = true;
            Debug.Log("<color=green>InventoryManager READY</color>");
            OnInventoryReady?.Invoke(this, EventArgs.Empty);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            var item = ItemManager.Instance.GetRandomItem();
            AddItem(item);   
            Debug.Log($"<color=green>Thêm {item}</color>");
        }
    }

    #region WEAPON OPERATIONS 
    
    /// <summary>
    /// Thêm weapon vào inventory. 
    /// Nếu đã có weapon cùng weaponName thì KHÔNG thêm duplicate.
    /// </summary>
    public void AddWeapon(Weapon runtimeWeapon, bool autoEquip = true)
    {
        if (runtimeWeapon == null) return;

        // Kiểm tra đã có weapon với weaponName này chưa
        Weapon existingWeapon = ownedWeapons.FirstOrDefault(w => w.weaponName == runtimeWeapon.weaponName);
        
        if (existingWeapon != null)
        {
            // Đã có → KHÔNG thêm duplicate, chỉ log
            Debug.Log($"<color=yellow>[InventoryManager] Weapon '{runtimeWeapon.weaponName}' đã tồn tại trong inventory → Không thêm duplicate</color>");
            return;
        }

        // Chưa có → Thêm mới
        ownedWeapons.Add(runtimeWeapon);
        Debug.Log($"<color=green>[InventoryManager] Thêm weapon mới: {runtimeWeapon.weaponName}</color>");

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

    /// <summary>
    /// Kiểm tra đã có weapon với weaponName này chưa
    /// </summary>
    public bool HasWeapon(Weapon w)
    {
        if (w == null) return false;
        return ownedWeapons.Any(owned => owned.weaponName == w.weaponName);
    }
    
    /// <summary>
    /// Lấy weapon runtime instance từ weaponName
    /// </summary>
    public Weapon GetWeaponByName(string weaponName)
    {
        return ownedWeapons.FirstOrDefault(w => w.weaponName == weaponName);
    }
    
    #endregion
    
    #region BUFF OPERATIONS 
    
    /// <summary>
    /// Thêm buff vào inventory.
    /// Nếu đã có buff cùng buffId thì KHÔNG thêm duplicate.
    /// </summary>
    public void AddBuff(BookBuff runtimeBookBuff, bool autoEquip = true)
    {
        if(runtimeBookBuff == null) return;
        
        // Kiểm tra đã có buff với buffId này chưa
        if (HasBookBuff(runtimeBookBuff))
        {
            Debug.Log($"<color=yellow>[InventoryManager] Buff '{runtimeBookBuff.buffId}' đã tồn tại trong inventory → Không thêm duplicate</color>");
            return;
        }

        // Chưa có → Thêm mới
        ownedBookBuffs.Add(runtimeBookBuff);
        Debug.Log($"<color=green>[InventoryManager] Thêm buff mới: {runtimeBookBuff.buffId}</color>");

        if(autoEquip && activeBookBuffs.Count < maxActiveBookBuffs)
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
    
    /// <summary>
    /// Lấy buff runtime instance từ buffId
    /// </summary>
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
    public void AddItem(Item runtimeItem, int count = 1, bool autoEquip = true)
    {
        if (runtimeItem == null || count <= 0) return;

        bool wasOwned = ownedItems.ContainsKey(runtimeItem);
        int previousCount = wasOwned ? ownedItems[runtimeItem] : 0;

        if (!runtimeItem.isStackable && wasOwned) return;

        ownedItems[runtimeItem] = previousCount + count;

        bool isActive = activeItems.Contains(runtimeItem);

        if (autoEquip && !isActive && activeItems.Count < maxActiveItem)
        {
            activeItems.Add(runtimeItem);
            ItemManager.Instance.ApplyItemEffect(runtimeItem, ownedItems[runtimeItem]);
            OnActiveItemsChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (isActive)
        {
            ItemManager.Instance.ApplyItemEffect(runtimeItem, count);
        }

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItem(Item item, int count = 1)
    {
        if (item == null || count <= 0 || !ownedItems.ContainsKey(item)) return;

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
        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UnequipItem(Item item)
    {
        if (item == null || !activeItems.Remove(item)) return;

        ItemManager.Instance.RemoveAllForItem(item);
        OnActiveItemsChanged?.Invoke(this, EventArgs.Empty);
        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HasItem(Item item)
    {
        return ownedItems.ContainsKey(item) && ownedItems[item] > 0;
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

    private void OnEnable()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnChestInteracted += HandleChestInteracted;
            Debug.Log("<color=magenta>[InventoryManager]</color> Đăng ký lắng nghe sự kiện OnChestInteracted");
        }
    }
    
    private void OnDisable()
    {   
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnChestInteracted -= HandleChestInteracted;
            Debug.Log("<color=magenta>[InventoryManager]</color> Hủy đăng ký lắng nghe sự kiện OnChestInteracted");
        }
    }
    
    private void HandleChestInteracted(object sender, GameEventManager.OnChestInteractEventArgs e)
    {
        AddItem(e.item);
        Debug.Log($"<color=magenta>[InventoryManager]</color> Nhận vật phẩm từ Chest: {e.item}");
    }
    #endregion
}