using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// LevelUpUI (cập nhật):
/// - UpgradeOption giờ có targetWeapon để hiển thị icon cho nâng cấp vũ khí cụ thể.
/// - Khi tạo choices: ưu tiên tạo WeaponUpgrade cho vũ khí đang sở hữu,
///   nếu không đủ thì đưa vũ khí chưa có (as drop) hoặc buff chưa có.
/// - Mỗi option có icon: nếu WeaponUpgrade => targetWeapon.icon; nếu Buff => try placeholder.
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    [Header("References")]
    public LevelSystem levelSystem;
    public InventoryManager inventoryManager;
    public GameObject panel;
    public LevelUpOptionUI optionPrefab;
    public Transform optionsParent;
    public int optionCount = 3;

    [Header("Weapon pool (choose from this)")]
    public WeaponData[] weaponPool;

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
        if (levelSystem == null)
            levelSystem = FindFirstObjectByType<LevelSystem>();

        if (inventoryManager == null && InventoryManager.Instance != null)
            inventoryManager = InventoryManager.Instance;

        if (levelSystem != null)
            levelSystem.OnLevelUp += OnLevelUp;

        if (panel != null) panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (levelSystem != null) levelSystem.OnLevelUp -= OnLevelUp;
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
        List<WeaponData> allWeapons = (weaponPool != null) ? weaponPool.Where(x => x != null).ToList() : new List<WeaponData>();
        List<WeaponData> owned = (inventoryManager != null) ? inventoryManager.ownedWeapons.Where(x => x != null).ToList() : new List<WeaponData>();
        List<WeaponData> unowned = allWeapons.Except(owned).ToList();

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
            // create a runtime ItemData (buff) and add
            ItemData buff = ScriptableObject.CreateInstance<ItemData>();
            buff.itemName = $"{option.buffType}_{option.tier}";
            buff.itemType = ItemType.Buff;
            buff.buffValue = GetBuffValueForTier(option.tier);
            buff.displayName = $"{option.tier} {option.buffType}";
            // Optionally set an icon: try to use a placeholder weapon icon if available
            if (option.targetWeapon != null) buff.icon = option.targetWeapon.icon;
            else if (weaponPool != null && weaponPool.Length > 0) buff.icon = weaponPool[rnd.Next(weaponPool.Length)].icon;

            inventoryManager.AddItem(buff);
        }
        else if (option.kind == UpgradeOption.Kind.WeaponUpgrade)
        {
            // apply upgrade to the player-owned weapon (clone)
            var target = option.targetWeapon;
            if (target != null)
            {
                WeaponData clone = ScriptableObject.Instantiate(target);
                clone.displayName = $"{target.displayName} +{option.tier}";
                ApplyUpgradeToWeapon(clone, option.tier);
                inventoryManager.AddWeapon(clone, equipIfSpace: true);
            }
        }
        else if (option.kind == UpgradeOption.Kind.WeaponDrop)
        {
            // simply add the weapon (drop) if player doesn't have it
            var w = option.targetWeapon;
            if (w != null && !inventoryManager.ownedWeapons.Contains(w))
            {
                inventoryManager.AddWeapon(w, equipIfSpace: true);
            }
        }

        CloseOptions();
    }

    void ApplyUpgradeToWeapon(WeaponData w, UpgradeTier tier)
    {
        int statCount = (tier == UpgradeTier.Common || tier == UpgradeTier.Uncommon) ? 1 : 2;
        float pct = TierPercent(tier);

        List<Action> statAppliers = new List<Action>
        {
            () => { w.baseDamage *= (1f + pct); },
            () => { w.attackRate *= (1f + pct); },
            () => { w.range *= (1f + pct); }
        };

        for (int i = 0; i < statCount && statAppliers.Count > 0; i++)
        {
            int idx = rnd.Next(0, statAppliers.Count);
            statAppliers[idx]();
            statAppliers.RemoveAt(idx);
        }
    }

    float TierPercent(UpgradeTier tier)
    {
        switch (tier)
        {
            case UpgradeTier.Common: return commonPercent;
            case UpgradeTier.Uncommon: return uncommonPercent;
            case UpgradeTier.Rare: return rarePercent;
            case UpgradeTier.Epic: return epicPercent;
            case UpgradeTier.Legendary: return legendaryPercent;
        }
        return commonPercent;
    }

    float GetBuffValueForTier(UpgradeTier tier)
    {
        switch (tier)
        {
            case UpgradeTier.Common: return 5f;
            case UpgradeTier.Uncommon: return 10f;
            case UpgradeTier.Rare: return 20f;
            case UpgradeTier.Epic: return 40f;
            case UpgradeTier.Legendary: return 80f;
        }
        return 0f;
    }

    UpgradeTier PickRandomTierForDisplay()
    {
        // simple distribution: mostly common/uncommon, rare+ rarer
        int r = rnd.Next(0, 100);
        if (r < 50) return UpgradeTier.Common;
        if (r < 75) return UpgradeTier.Uncommon;
        if (r < 90) return UpgradeTier.Rare;
        if (r < 98) return UpgradeTier.Epic;
        return UpgradeTier.Legendary;
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
        public WeaponData targetWeapon; // if set, UI can show its icon
    }

    public enum BuffType { Health, Luck, Damage, AttackSpeed }
}
