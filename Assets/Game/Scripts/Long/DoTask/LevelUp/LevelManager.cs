// LevelManager.cs (moved logic from LevelUpUI/LevelUpOptionUI here; uses EventHandler for callbacks)
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Progression")]
    public int level = 1;
    public float currentXP = 0f;
    public float xpToNext = 100f;
    public float growthFactor = 1.5f;

    // Event:
    // - OnCreated: bắn khi Instance đã được gán xong (UI dùng để đăng ký OnLevelUp)
    // - OnLevelUp: gửi mỗi khi lên level
    public static event EventHandler OnCreated;
    public event EventHandler<LevelUpEventArgs> OnLevelUp; // Now sends options

    // New event for when an upgrade is selected (replaces Action)
    public event EventHandler<UpgradeSelectedEventArgs> OnUpgradeSelected;

    // Database reference (moved from LevelUpUI)
    private UpgradeDatabase upgradeDB;

    System.Random rnd = new System.Random();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        upgradeDB = UpgradeDatabase.Instance; // Assuming this exists

        // Báo cho LevelUpUI biết rằng LevelManager đã sẵn sàng
        OnCreated?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        // TEST nâng cấp bằng phím U 
        // (Bạn có thể xoá nếu không cần)
        if (Input.GetKeyDown(KeyCode.U))
            AddXP(100);
    }

    /// <summary>
    /// Thêm XP và xử lý logic lên cấp
    /// </summary>
    public void AddXP(float amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            level++;

            // tăng mức XP cần cho level tiếp theo
            xpToNext *= growthFactor;

            // Generate options and invoke event (moved logic here)
            List<UpgradeOption> options = GenerateUpgradeOptions();

            // báo UI rằng đã lên level mới với options
            OnLevelUp?.Invoke(this, new LevelUpEventArgs { Level = level, Options = options });
        }
    }

    /// <summary>
    /// Lấy tỉ lệ % XP để hiện UI
    /// </summary>
    public float GetProgress01()
    {
        if (xpToNext <= 0) return 0f;
        return Mathf.Clamp01(currentXP / xpToNext);
    }

    // Moved from LevelUpUI: Generate random upgrade options
    private List<UpgradeOption> GenerateUpgradeOptions(int optionCount = 3)
    {
        InventoryManager inventoryManager = InventoryManager.Instance; // Assuming access

        List<Weapon> allWeapons = upgradeDB.allWeapons.ToList();
        List<Weapon> owned = inventoryManager?.ownedWeapons.Where(x => x != null).ToList() ?? new();
        List<Weapon> unowned = allWeapons.Except(owned).ToList();

        List<UpgradeOption> candidates = new();

        foreach (var w in owned)
        {
            candidates.Add(new UpgradeOption
            {
                kind = UpgradeOption.Kind.WeaponUpgrade,
                tier = PickRandomTierForDisplay(),
                targetWeapon = w
            });
        }

        foreach (var w in unowned)
        {
            candidates.Add(new UpgradeOption
            {
                kind = UpgradeOption.Kind.WeaponDrop,
                tier = UpgradeTier.Common,
                targetWeapon = w
            });
        }

        foreach (var buff in upgradeDB.allBuffs)
        {
            candidates.Add(new UpgradeOption
            {
                kind = UpgradeOption.Kind.Buff,
                tier = PickRandomTierForDisplay(),
                targetBuff = buff
            });
        }

        candidates = candidates.OrderBy(x => rnd.Next()).ToList();

        List<UpgradeOption> chosen = new();
        foreach (var c in candidates)
        {
            if (chosen.Count >= optionCount) break;

            bool duplicate = chosen.Any(ch =>
                (c.kind == UpgradeOption.Kind.WeaponUpgrade && ch.targetWeapon == c.targetWeapon) ||
                (c.kind == UpgradeOption.Kind.WeaponDrop    && ch.targetWeapon == c.targetWeapon) ||
                (c.kind == UpgradeOption.Kind.Buff          && ch.targetBuff    == c.targetBuff)
            );

            if (!duplicate)
                chosen.Add(c);
        }

        return chosen;
    }

    // Moved from LevelUpUI: Handle selected upgrade (called by UI via event)
    public void ApplyUpgrade(UpgradeOption option)
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null) return;

        Rarity rarity = (Rarity)Enum.Parse(typeof(Rarity), option.tier.ToString());

        switch (option.kind)
        {
            // ───────────────────────────────────────────────
            // 1) UPGRADE VŨ KHÍ ĐANG CÓ
            // ───────────────────────────────────────────────
            case UpgradeOption.Kind.WeaponUpgrade:
                if (option.targetWeapon != null)
                {
                    // Tìm bản runtime trong inventory (không dùng template)
                    Weapon runtimeWeapon = inventoryManager.ownedWeapons
                        .FirstOrDefault(w => w.weaponName == option.targetWeapon.weaponName);

                    if (runtimeWeapon != null)
                    {
                        // Nâng cấp đúng bản runtime đang dùng
                        WeaponUpgrade.Upgrade(runtimeWeapon, rarity);
                    }
                    else
                    {
                        // Nếu chưa có → thêm mới bằng clone
                        Weapon newRuntime = Instantiate(option.targetWeapon);
                        inventoryManager.AddWeapon(newRuntime);
                        WeaponManager.Instance.AddWeapon(newRuntime);
                    }
                }
                break;

            // ───────────────────────────────────────────────
            // 2) NHẶT VŨ KHÍ MỚI (PHẢI CLONE SCRIPTABLEOBJECT)
            // ───────────────────────────────────────────────
            case UpgradeOption.Kind.WeaponDrop:
                if (!inventoryManager.HasWeapon(option.targetWeapon))
                {
                    // Clone thành runtime weapon trước khi thêm
                    Weapon newRuntime = Instantiate(option.targetWeapon);
                    inventoryManager.AddWeapon(newRuntime);
                    WeaponManager.Instance.AddWeapon(newRuntime);
                }
                break;

            // ───────────────────────────────────────────────
            // 3) BUFF (nếu bạn còn dùng)
            // ───────────────────────────────────────────────
            case UpgradeOption.Kind.Buff:
            {
                // Kiểm tra nhân vật đã từng có buff này chưa
                BookBuff runtimeBuff = inventoryManager.ownedBookBuffs
                    .FirstOrDefault(b => b.buffId == option.targetBuff.buffId);

                if (runtimeBuff == null)
                {
                    // LẦN ĐẦU NHẬN BUFF → CLONE VÀ LEVELUP 1 LẦN
                    runtimeBuff = Instantiate(option.targetBuff);
                    runtimeBuff.level = 0; // đảm bảo reset template

                    runtimeBuff.LevelUp(rarity);

                    inventoryManager.AddBuff(runtimeBuff);

                    // APPLY VÀO PLAYER
                    BookBuffManager.Instance.ApplyBuff(runtimeBuff);

                    Debug.Log($"Nhận buff mới: {runtimeBuff.name}, level {runtimeBuff.level}");
                }
                else
                {
                    // ĐÃ CÓ → TĂNG LEVEL + APPLY LẠI
                    runtimeBuff.LevelUp(rarity);

                    BookBuffManager.Instance.ApplyBuff(runtimeBuff);

                    Debug.Log($"Buff {runtimeBuff.name} tăng cấp lên {runtimeBuff.level}");
                }
            }
            break;
        }

        // Invoke event if needed for UI to close, etc.
        OnUpgradeSelected?.Invoke(this, new UpgradeSelectedEventArgs { SelectedOption = option });
    }

    UpgradeTier PickRandomTierForDisplay()
    {
        Rarity random = RarityHelper.GetRandomRarity();
        return (UpgradeTier)Enum.Parse(typeof(UpgradeTier), random.ToString());
    }

    public enum UpgradeTier { Common, Uncommon, Rare, Epic, Legendary }

    [Serializable]
    public class UpgradeOption
    {
        public enum Kind { WeaponUpgrade, WeaponDrop, Buff }

        public Kind kind;
        public UpgradeTier tier;
        public Weapon targetWeapon;
        public BookBuff targetBuff;
    }

    public class LevelUpEventArgs : EventArgs
    {
        public int Level { get; set; }
        public List<UpgradeOption> Options { get; set; }
    }

    public class UpgradeSelectedEventArgs : EventArgs
    {
        public UpgradeOption SelectedOption { get; set; }
    }
}