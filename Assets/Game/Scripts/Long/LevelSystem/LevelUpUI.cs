using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class LevelUpUI : MonoBehaviour
{
    [Header("References")]
    public LevelManager levelManager;
    public InventoryManager inventoryManager;
    public GameObject panel;
    public LevelUpOptionUI optionPrefab;
    public Transform optionsParent;
    public int optionCount = 3;

    [Header("Weapon pool (choose from this)")]
    public Weapon[] weaponPool;

    [Header("Upgrade settings")]
    public float commonPercent = 0.08f;
    public float uncommonPercent = 0.12f;
    public float rarePercent = 0.18f;
    public float epicPercent = 0.28f;
    public float legendaryPercent = 0.45f;

    List<LevelUpOptionUI> spawnedOptions = new List<LevelUpOptionUI>();
    bool isShowing = false;
    System.Random rnd = new System.Random();

    void Start()
    {
        if (levelManager == null)
            levelManager = FindFirstObjectByType<LevelManager>();

        if (inventoryManager == null && InventoryManager.Instance != null)
            inventoryManager = InventoryManager.Instance;

        if (levelManager != null)
            levelManager.OnLevelUp += OnLevelUp;

        if (panel != null) panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (levelManager != null) levelManager.OnLevelUp -= OnLevelUp;
    }

    void OnLevelUp(int newLevel)
    {
        ShowOptions();
    }

    public void ShowOptions()
    {
        var MouseActive = FindFirstObjectByType<TPCameraController>();
        MouseActive.isUIOpen = true;
        if (isShowing) return;
        isShowing = true;

        if (panel != null) panel.SetActive(true);

        foreach (var o in spawnedOptions) Destroy(o.gameObject);
        spawnedOptions.Clear();

        // Prepare pools
        List<Weapon> allWeapons = (weaponPool != null) ? weaponPool.Where(x => x != null).ToList() : new List<Weapon>();
        List<Weapon> owned = (inventoryManager != null) ? inventoryManager.ownedWeapons.Where(x => x != null).ToList() : new List<Weapon>();
        List<Weapon> unowned = allWeapons.Except(owned).ToList();

        // Buff types (example)
        var allBuffTypes = Enum.GetValues(typeof(BuffType)).Cast<BuffType>().ToList();

        // We'll assemble a list of candidate UpgradeOption objects
        List<UpgradeOption> candidates = new List<UpgradeOption>();

        // 1) Weapon upgrade candidates for owned weapons (so they show icon and are valid upgrades)
        foreach (var w in owned)
        {
            candidates.Add(new UpgradeOption
            {
                kind = UpgradeOption.Kind.WeaponUpgrade,
                tier = PickRandomTierForDisplay(),
                targetWeapon = w
            });
        }

        // 2) Add some unowned weapons as "weapon drops" (these will show icon too)
        foreach (var w in unowned)
        {
            candidates.Add(new UpgradeOption
            {
                kind = UpgradeOption.Kind.WeaponDrop,
                tier = UpgradeTier.Common,
                targetWeapon = w
            });
        }

        // 3) Buff candidates (for buffs we don't yet have; as buff system chưa có, we'll still create them)
        foreach (var b in allBuffTypes)
        {
            candidates.Add(new UpgradeOption
            {
                kind = UpgradeOption.Kind.Buff,
                tier = PickRandomTierForDisplay(),
                buffType = b,
                targetWeapon = null
            });
        }

        // Shuffle candidates
        candidates = candidates.OrderBy(x => rnd.Next()).ToList();

        // Pick up to optionCount distinct choices. Avoid duplicate targetWeapon + same buff.
        List<UpgradeOption> chosen = new List<UpgradeOption>();
        foreach (var c in candidates)
        {
            if (chosen.Count >= optionCount) break;

            // skip duplicates: same kind+targetWeapon or same buffType
            bool duplicate = false;
            foreach (var ch in chosen)
            {
                if (c.kind == UpgradeOption.Kind.WeaponUpgrade || c.kind == UpgradeOption.Kind.WeaponDrop)
                {
                    if (ch.targetWeapon != null && c.targetWeapon == ch.targetWeapon) duplicate = true;
                }
                else if (c.kind == UpgradeOption.Kind.Buff)
                {
                    if (ch.kind == UpgradeOption.Kind.Buff && ch.buffType == c.buffType) duplicate = true;
                }
            }
            if (duplicate) continue;

            chosen.Add(c);
        }

        // If not enough chosen (edge cases), pad with generic options
        int safety = 0;
        while (chosen.Count < optionCount && safety++ < 20)
        {
            // prefer unowned weapons as filler
            var filler = unowned.OrderBy(x => rnd.Next()).FirstOrDefault();
            if (filler != null)
            {
                var opt = new UpgradeOption { kind = UpgradeOption.Kind.WeaponDrop, tier = UpgradeTier.Common, targetWeapon = filler };
                if (!chosen.Any(ch => ch.targetWeapon == opt.targetWeapon)) chosen.Add(opt);
            }
            else
            {
                // fallback buff
                var b = allBuffTypes[rnd.Next(allBuffTypes.Count)];
                if (!chosen.Any(ch => ch.kind == UpgradeOption.Kind.Buff && ch.buffType == b))
                    chosen.Add(new UpgradeOption { kind = UpgradeOption.Kind.Buff, tier = UpgradeTier.Common, buffType = b });
            }
        }

        // instantiate UI for each chosen option
        foreach (var opt in chosen)
        {
            var inst = Instantiate(optionPrefab, optionsParent);
            inst.Setup(opt, OnUpgradeOptionSelected);
            spawnedOptions.Add(inst);
        }
    }

    // When player clicks an upgrade option
    void OnUpgradeOptionSelected(UpgradeOption option)
    {
        if (inventoryManager == null)
        {
            CloseOptions();
            return;
        }

        if (option.kind == UpgradeOption.Kind.Buff)
        {
            // Tạo instance Buff (kế thừa từ Weapon)
            Buff buff = ScriptableObject.CreateInstance<Buff>();
            buff.weaponName = $"{option.tier} {option.buffType}";
            buff.buffType = (BuffType)option.buffType;
            // Optionally set an icon: try to use a placeholder weapon icon if available
            if (option.targetWeapon != null) buff.Icon = option.targetWeapon.Icon;
            else if (weaponPool != null && weaponPool.Length > 0) buff.Icon = weaponPool[rnd.Next(weaponPool.Length)].Icon;

            // Chuyển đổi tier sang Rarity và upgrade sử dụng WeaponUpgrade
            Rarity rarity = (Rarity)Enum.Parse(typeof(Rarity), option.tier.ToString());
            WeaponUpgrade.Upgrade(buff, rarity);

            inventoryManager.AddWeapon(buff);
        }
        else if (option.kind == UpgradeOption.Kind.WeaponUpgrade)
        {
            // apply upgrade to the player-owned weapon using WeaponUpgrade
            var target = option.targetWeapon;
            if (target != null)
            {
                // Chuyển đổi tier sang Rarity (giả sử enum tương đồng)
                Rarity rarity = (Rarity)Enum.Parse(typeof(Rarity), option.tier.ToString());
                WeaponUpgrade.Upgrade(target, rarity);
                // Không cần clone vì LevelUp sẽ cập nhật trực tiếp trên weapon hiện có
            }
        }
        else if (option.kind == UpgradeOption.Kind.WeaponDrop)
        {
            // simply add the weapon (drop) if player doesn't have it
            var w = option.targetWeapon;
            if (w != null && !inventoryManager.ownedWeapons.Contains(w))
            {
                inventoryManager.AddWeapon(w);
            }
        }

        CloseOptions();
    }

    UpgradeTier PickRandomTierForDisplay()
    {
        // Sử dụng RarityHelper để random tier (chuyển sang UpgradeTier)
        Rarity randomRarity = RarityHelper.GetRandomRarity();
        return (UpgradeTier)Enum.Parse(typeof(UpgradeTier), randomRarity.ToString());
    }

    public void CloseOptions()
    {
        var MouseActive = FindFirstObjectByType<TPCameraController>();
        MouseActive.isUIOpen = false;
        if (!isShowing) return;
        isShowing = false;
        if (panel != null) panel.SetActive(false);
    }

    // Types
    public enum UpgradeTier { Common, Uncommon, Rare, Epic, Legendary }

    [Serializable]
    public class UpgradeOption
    {
        public enum Kind { WeaponUpgrade, WeaponDrop, Buff }
        public Kind kind;
        public UpgradeTier tier;
        public BuffType buffType;
        public Weapon targetWeapon; // if set, UI can show its icon
    }
}