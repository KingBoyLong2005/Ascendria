using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static LootDropManager;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    // ================= EVENTS =================
    public static event EventHandler OnCreated;
    public event EventHandler<LevelUpEventArgs> OnLevelUp;
    public event EventHandler<UpgradeSelectedEventArgs> OnUpgradeApplied;
    public event EventHandler<XPProgressEventArgs> OnXPChanged;

    [Header("Level Progression")]
    public float Luck ; // Dùng đê tăng luck quay ra độ hiếm, 0->1 (truyền luck vào getrandom dòng 237)
    public int level = 1;
    public float currentXP = 0f;
    public float xpToNext = 100f;
    public float growthFactor = 1.2f;
    
    UpgradeDatabase upgradeDB;
    System.Random rnd = new System.Random();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        upgradeDB = UpgradeDatabase.Instance;

        OnCreated?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            AddXP(50);
        }
    }

    // ================= XP =================
    public void AddXP(float amount)
    {
        currentXP += amount;

        OnXPChanged?.Invoke(this, new XPProgressEventArgs(currentXP, xpToNext));

        while (currentXP >= xpToNext)
        {

            currentXP -= xpToNext;
            level++;

            float oldXPToNext = xpToNext;
            xpToNext *= growthFactor;

            List<UpgradeOption> options = GenerateUpgradeOptions();

            OnLevelUp?.Invoke(this, new LevelUpEventArgs(level, options));
            OnXPChanged?.Invoke(this, new XPProgressEventArgs(currentXP, xpToNext));
        }
}


    public float GetProgress01()
    {
        return Mathf.Clamp01(currentXP / xpToNext);
    }

    // ================= APPLY =================
    public void ApplyUpgrade(UpgradeOption option)
    {
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null) return;

        Rarity rarity = (Rarity)Enum.Parse(typeof(Rarity), option.tier.ToString());

        switch (option.kind)
        {
            case UpgradeOption.Kind.WeaponUpgrade:
                ApplyWeaponUpgrade(inv, option, rarity);
                break;

            case UpgradeOption.Kind.WeaponDrop:
                ApplyWeaponDrop(inv, option);
                break;

            case UpgradeOption.Kind.Buff:
                ApplyBuff(inv, option, rarity);
                break;
        }

        OnUpgradeApplied?.Invoke(this, new UpgradeSelectedEventArgs(option));
    }

    // ================= INTERNAL =================
    List<UpgradeOption> GenerateUpgradeOptions(int count = 3)
    {
        InventoryManager inv = InventoryManager.Instance;
        List<UpgradeOption> pool = new List<UpgradeOption>();

        // 1. Weapon Upgrades (cho weapons đã có)
        foreach (var w in inv.ownedWeapons)
        {
            pool.Add(new UpgradeOption(
                UpgradeOption.Kind.WeaponUpgrade,
                PickTier(),
                w,
                null));
        }

        // 2. Weapon Drops (cho weapons CHƯA có)
        foreach (var w in upgradeDB.allWeapons)
        {
            if (!inv.HasWeapon(w))
            {
                pool.Add(new UpgradeOption(
                    UpgradeOption.Kind.WeaponDrop,
                    UpgradeTier.Common,
                    w,
                    null));
            }
        }

        // 3. Buffs (cả đã có và chưa có)
        foreach (var b in upgradeDB.allBuffs)
        {
            pool.Add(new UpgradeOption(
                UpgradeOption.Kind.Buff,
                PickTier(),
                null,
                b));
        }

        // Shuffle
        pool = pool.OrderBy(x => rnd.Next()).ToList();

        // Distinct và lấy count options
        List<UpgradeOption> result = new List<UpgradeOption>();
        foreach (var o in pool)
        {
            if (result.Count >= count) break;

            bool duplicate = result.Any(r =>
                r.kind == o.kind &&
                r.targetWeapon == o.targetWeapon &&
                r.targetBuff == o.targetBuff);

            if (!duplicate)
                result.Add(o);
        }

        return result;
    }

    /// <summary>
    /// Nâng cấp weapon ĐÃ CÓ trong inventory
    /// </summary>
    void ApplyWeaponUpgrade(InventoryManager inv, UpgradeOption o, Rarity r)
    {
        // Tìm weapon runtime instance từ weaponName
        Weapon runtime = inv.GetWeaponByName(o.targetWeapon.weaponName);

        if (runtime != null)
        {
            // Đã có → Chỉ upgrade level, KHÔNG thêm vào inventory
            WeaponUpgrade.Upgrade(runtime, r);
            Debug.Log($"<color=cyan>[LevelManager]</color> Upgrade weapon đã có: {runtime.weaponName} → Level {runtime.level}");
        }
        else
        {
            // Không tìm thấy → Có thể weapon bị remove, thêm lại
            Debug.LogWarning($"<color=orange>[LevelManager]</color> Weapon '{o.targetWeapon.weaponName}' không tìm thấy trong inventory, thêm lại");
            ApplyWeaponDrop(inv, o);
        }
    }

    /// <summary>
    /// Thêm weapon MỚI vào inventory (chỉ khi CHƯA có)
    /// </summary>
    void ApplyWeaponDrop(InventoryManager inv, UpgradeOption o)
    {
        // Kiểm tra lần cuối để tránh duplicate
        if (inv.HasWeapon(o.targetWeapon))
        {
            Debug.LogWarning($"<color=red>[LevelManager]</color> Weapon '{o.targetWeapon.weaponName}' đã có trong inventory → Bỏ qua thêm");
            return;
        }

        Weapon runtime = Instantiate(o.targetWeapon);
        inv.AddWeapon(runtime);
        WeaponManager.Instance.AddWeapon(runtime);
        
        Debug.Log($"<color=green>[LevelManager]</color> Thêm weapon MỚI: {runtime.weaponName}");
    }

    /// <summary>
    /// Apply hoặc nâng cấp buff
    /// </summary>
    void ApplyBuff(InventoryManager inv, UpgradeOption o, Rarity r)
    {
        // Tìm buff runtime instance từ buffId
        BookBuff runtime = inv.GetBuffById(o.targetBuff.buffId);

        if (runtime == null)
        {
            // Chưa có → Tạo mới và thêm vào inventory
            runtime = Instantiate(o.targetBuff);
            runtime.level = 0;
            inv.AddBuff(runtime);
            Debug.Log($"<color=green>[LevelManager]</color> Thêm buff MỚI: {runtime.buffId}");
        }
        else
        {
            Debug.Log($"<color=cyan>[LevelManager]</color> Nâng cấp buff đã có: {runtime.buffId}");
        }

        // Nâng cấp level
        runtime.LevelUp(r);
        BookBuffManager.Instance.ApplyBuff(runtime);
        
        Debug.Log($"<color=cyan>[LevelManager]</color> Buff {runtime.buffId} → Level {runtime.level}");
    }

    UpgradeTier PickTier()
    {
        return (UpgradeTier)Enum.Parse(
            typeof(UpgradeTier),
            RarityHelper.GetRandomRarity().ToString()); // sẽ chuyền luck vào GetRandomRarity()
            
    }

    // ================= DATA =================
    public enum UpgradeTier
    {
        Common, Uncommon, Rare, Epic, Legendary
    }

    [Serializable]
    public class UpgradeOption
    {
        public enum Kind
        {
            WeaponUpgrade,  // Nâng cấp weapon ĐÃ CÓ
            WeaponDrop,     // Thêm weapon MỚI
            Buff            // Buff (cả mới và nâng cấp)
        }

        public Kind kind;
        public UpgradeTier tier;
        public Weapon targetWeapon;
        public BookBuff targetBuff;

        public UpgradeOption(Kind kind, UpgradeTier tier, Weapon weapon, BookBuff buff)
        {
            this.kind = kind;
            this.tier = tier;
            this.targetWeapon = weapon;
            this.targetBuff = buff;
        }
    }

    public class LevelUpEventArgs : EventArgs
    {
        public int Level;
        public List<UpgradeOption> Options;

        public LevelUpEventArgs(int level, List<UpgradeOption> options)
        {
            Level = level;
            Options = options;
        }
    }

    public class XPProgressEventArgs : EventArgs
    {
        public float currXP;
        public float xpToNext;
        
        public XPProgressEventArgs(float currentXP, float xpToNextLvl)
        {
            currXP = currentXP;
            xpToNext = xpToNextLvl;
        }
    }

    public class UpgradeSelectedEventArgs : EventArgs
    {
        public UpgradeOption Option;

        public UpgradeSelectedEventArgs(UpgradeOption option)
        {
            Option = option;
        }
    }
}