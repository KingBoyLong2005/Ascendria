using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    // ================= EVENTS =================
    public static event EventHandler OnCreated;
    public event EventHandler<LevelUpEventArgs> OnLevelUp;
    public event EventHandler<UpgradeSelectedEventArgs> OnUpgradeApplied;

    [Header("Level Progression")]
    public int level = 1;
    public float currentXP = 0f;
    public float xpToNext = 100f;
    public float growthFactor = 1.5f;
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
            AddXP(100);
        }
    }

    // ================= XP =================
    public void AddXP(float amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            level++;
            xpToNext *= growthFactor;

            List<UpgradeOption> options = GenerateUpgradeOptions();
            OnLevelUp?.Invoke(this, new LevelUpEventArgs(level, options));
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

        Rarity rarity =
            (Rarity)Enum.Parse(typeof(Rarity), option.tier.ToString());

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

        OnUpgradeApplied?.Invoke(
            this,
            new UpgradeSelectedEventArgs(option));
    }

    // ================= INTERNAL =================
    List<UpgradeOption> GenerateUpgradeOptions(int count = 3)
    {
        InventoryManager inv = InventoryManager.Instance;
        List<UpgradeOption> pool = new List<UpgradeOption>();

        foreach (var w in inv.ownedWeapons)
        {
            pool.Add(new UpgradeOption(
                UpgradeOption.Kind.WeaponUpgrade,
                PickTier(),
                w,
                null));
        }

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

        foreach (var b in upgradeDB.allBuffs)
        {
            pool.Add(new UpgradeOption(
                UpgradeOption.Kind.Buff,
                PickTier(),
                null,
                b));
        }

        // shuffle
        pool = pool.OrderBy(x => rnd.Next()).ToList();

        // manual distinct
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

    void ApplyWeaponUpgrade(
        InventoryManager inv,
        UpgradeOption o,
        Rarity r)
    {
        Weapon runtime = inv.ownedWeapons
            .FirstOrDefault(w => w.weaponName == o.targetWeapon.weaponName);

        if (runtime != null)
            WeaponUpgrade.Upgrade(runtime, r);
        else
            ApplyWeaponDrop(inv, o);
    }

    void ApplyWeaponDrop(
        InventoryManager inv,
        UpgradeOption o)
    {
        if (inv.HasWeapon(o.targetWeapon)) return;

        Weapon runtime = Instantiate(o.targetWeapon);
        inv.AddWeapon(runtime);
        WeaponManager.Instance.AddWeapon(runtime);
    }

    void ApplyBuff(
        InventoryManager inv,
        UpgradeOption o,
        Rarity r)
    {
        BookBuff runtime = inv.ownedBookBuffs
            .FirstOrDefault(b => b.buffId == o.targetBuff.buffId);

        if (runtime == null)
        {
            runtime = Instantiate(o.targetBuff);
            runtime.level = 0;
            inv.AddBuff(runtime);
        }

        runtime.LevelUp(r);
        BookBuffManager.Instance.ApplyBuff(runtime);
    }

    UpgradeTier PickTier()
    {
        return (UpgradeTier)Enum.Parse(
            typeof(UpgradeTier),
            RarityHelper.GetRandomRarity().ToString());
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
            WeaponUpgrade,
            WeaponDrop,
            Buff
        }

        public Kind kind;
        public UpgradeTier tier;
        public Weapon targetWeapon;
        public BookBuff targetBuff;

        public UpgradeOption(
            Kind kind,
            UpgradeTier tier,
            Weapon weapon,
            BookBuff buff)
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

    public class UpgradeSelectedEventArgs : EventArgs
    {
        public UpgradeOption Option;

        public UpgradeSelectedEventArgs(UpgradeOption option)
        {
            Option = option;
        }
    }
}
