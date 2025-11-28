using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CharacterStats (cải tiến)
/// - Hỗ trợ nạp profile (CharacterStarterProfile / CharacterProfile)
/// - Lưu extra stats trong dictionary
/// - ApplyBuff(ItemData) để áp dụng buff ngay (level up)
/// - Keep list of applied buffs (runtime) to avoid double-applying and allow removal later
/// - Event OnStatsChanged để UI/others lắng nghe
/// </summary>
public class CharacterStats : MonoBehaviour
{
    [Header("Profile Reference (ScriptableObject)")]
    public CharacterStaterProfile profile; // hoặc CharacterProfile nếu bạn đổi tên
    // private InventoryManager inventoryManager;

    [Header("Runtime Base Stats (copied from profile)")]
    public float maxHP = 100f;
    public float currentHP = 100f;
    public float moveSpeed = 5f;
    public float luck = 0f;

    // Extra stats map (non-serialized). Nếu muốn nhìn trong inspector, export/import từ listExtra.
    private Dictionary<string, float> extra = new Dictionary<string, float>();

    // Keep applied buff items (runtime) so we don't apply same buff twice and can remove if needed
    private List<ItemData> appliedBuffs = new List<ItemData>();

    // Optional: provide a serializable list mirror if you want to inspect default extra stats in inspector
    [Serializable]
    public struct ExtraStatEntry { public string key; public float value; }

    [Header("Editor: initial extra stats (copied to runtime at Start)")]
    public ExtraStatEntry[] listExtraDefaults;

    /// <summary>Event fired when stats changed (useful for UI updates)</summary>
    public event Action OnStatsChanged;

    void Awake()
    {
        // initialize from profile (if any) or from inspector defaults
        // if (profile != null) ApplyProfile(profile);
        // else ApplyDefaultsFromInspector();
    }

    void Start()
    {
        if (profile != null) ApplyProfile(profile);
        else ApplyDefaultsFromInspector();
        // ensure currentHP not exceeding max
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
    }

    void ApplyDefaultsFromInspector()
    {
        extra.Clear();
        if (listExtraDefaults != null)
        {
            foreach (var e in listExtraDefaults)
            {
                if (string.IsNullOrEmpty(e.key)) continue;
                extra[e.key] = e.value;
            }
        }
    }

    /// <summary>
    /// Apply profile ScriptableObject to runtime stats.
    /// Profile expected to hold base stats and extra stats (CharacterStarterProfile).
    /// Also optionally gives starting weapons (handled in profile apply).
    /// </summary>
    public void ApplyProfile(CharacterStaterProfile p)
    {
        profile = p;
        if (p == null)
        {
            Debug.LogWarning("CharacterStats.ApplyProfile called with null profile.");
            return;
        }

        // copy base stats
        maxHP = p.maxHP;
        currentHP = p.maxHP; // start full
        moveSpeed = p.moveSpeed;
        luck = p.luck;

        // copy extra stats
        extra.Clear();
        if (p.extraStats != null)
        {
            foreach (var s in p.extraStats)
            {
                if (string.IsNullOrEmpty(s.key)) continue;
                extra[s.key] = s.value;
            }
        }
        // Add starting weapons: safe access to InventoryManager.Instance
        
            InventoryManager.Instance.AddWeapon(p.startingWeapons, equipIfSpace: true);
        // If profile contains starting weapons (if your CharacterStarterProfile has such a field),
        // add them to inventory (non-destructive).
        #if UNITY_EDITOR
        // optional: debug log
        #endif

        RaiseStatsChanged();
    }

    /// <summary>
    /// Add amount to a named stat. Supports built-in keys "maxHP", "moveSpeed", "luck".
    /// Other keys go to extra[] map.
    /// </summary>
    public void AddToStat(string key, float amount)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (key == "maxHP")
        {
            maxHP += amount;
            // optional: increase currentHP proportionally or add same amount
            currentHP = Mathf.Min(currentHP + amount, maxHP);
        }
        else if (key == "moveSpeed")
        {
            moveSpeed += amount;
        }
        else if (key == "luck")
        {
            luck += amount;
        }
        else
        {
            if (!extra.ContainsKey(key)) extra[key] = 0f;
            extra[key] += amount;
        }

        RaiseStatsChanged();
    }

    /// <summary>
    /// Get a stat by key. Recognizes built-in keys; falls back to extra map.
    /// </summary>
    public float GetStat(string key)
    {
        if (string.IsNullOrEmpty(key)) return 0f;

        if (key == "maxHP") return maxHP;
        if (key == "moveSpeed") return moveSpeed;
        if (key == "luck") return luck;

        if (extra.TryGetValue(key, out float v)) return v;
        return 0f;
    }

    /// <summary>
    /// Apply a buff ItemData to this character immediately.
    /// If the item has statKey set, we use statKey; otherwise use buffType mapping.
    /// If the item was already applied (same instance/asset), it won't be applied again.
    /// </summary>
    public void ApplyBuff(ItemData item)
    {
        if (item == null) return;

        // Prevent double application of same asset/instance
        if (appliedBuffs.Contains(item))
        {
            Debug.Log($"Buff {item.displayName} already applied, skipping.");
            return;
        }

        // Prefer explicit statKey if provided
        if (!string.IsNullOrEmpty(item.statKey))
        {
            AddToStat(item.statKey, item.buffValue);
            appliedBuffs.Add(item);
            Debug.Log($"Applied buff (statKey) {item.displayName} -> {item.statKey} += {item.buffValue}");
            return;
        }

        // Map buffType to stats
        switch (item.buffType)
        {
            case BuffType.MaxHP:
                AddToStat("maxHP", item.buffValue);
                break;
            case BuffType.MoveSpeed:
                AddToStat("moveSpeed", item.buffValue);
                break;
            case BuffType.Luck:
                AddToStat("luck", item.buffValue);
                break;
            case BuffType.DamageFlat:
                AddToStat("damageFlat", item.buffValue);
                break;
            case BuffType.DamageMultiplier:
                // keep multiplier in extra as multiplicative factor (default 1)
                if (!extra.ContainsKey("damageMultiplier")) extra["damageMultiplier"] = 1f;
                extra["damageMultiplier"] *= (1f + item.buffValue); // e.g. buffValue=0.1 => *1.1
                RaiseStatsChanged();
                break;
            case BuffType.AttackSpeed:
                AddToStat("attackRate", item.buffValue);
                break;
            case BuffType.CritChance:
                AddToStat("critChance", item.buffValue);
                break;
            default:
                Debug.LogWarning($"ApplyBuff: Unknown buff type {item.buffType}");
                break;
        }

        // Remember applied buff (so user can't pick same buff twice)
        appliedBuffs.Add(item);
        RaiseStatsChanged();

        Debug.Log($"Applied buff {item.displayName} ({item.buffType}) value {item.buffValue}");
    }

    /// <summary>
    /// Remove an applied buff (useful for temporary buffs). Reverse operation must mirror ApplyBuff.
    /// For multiplicative buffs like DamageMultiplier, removal will divide by factor (assumes no other multipliers changed).
    /// </summary>
    public void RemoveBuff(ItemData item)
    {
        if (item == null) return;
        if (!appliedBuffs.Contains(item)) return;

        // Reverse mapping (note: be careful with multiplicative order if multiple multipliers exist)
        if (!string.IsNullOrEmpty(item.statKey))
        {
            AddToStat(item.statKey, -item.buffValue);
        }
        else
        {
            switch (item.buffType)
            {
                case BuffType.MaxHP:
                    AddToStat("maxHP", -item.buffValue);
                    break;
                case BuffType.MoveSpeed:
                    AddToStat("moveSpeed", -item.buffValue);
                    break;
                case BuffType.Luck:
                    AddToStat("luck", -item.buffValue);
                    break;
                case BuffType.DamageFlat:
                    AddToStat("damageFlat", -item.buffValue);
                    break;
                case BuffType.DamageMultiplier:
                    if (extra.ContainsKey("damageMultiplier"))
                    {
                        float factor = (1f + item.buffValue);
                        if (Mathf.Abs(factor) > 0.0001f) extra["damageMultiplier"] /= factor;
                    }
                    break;
                case BuffType.AttackSpeed:
                    AddToStat("attackRate", -item.buffValue);
                    break;
                case BuffType.CritChance:
                    AddToStat("critChance", -item.buffValue);
                    break;
            }
        }

        appliedBuffs.Remove(item);
        RaiseStatsChanged();
    }

    /// <summary>
    /// Check whether a specific buff item was already applied
    /// </summary>
    public bool HasAppliedBuff(ItemData item) => appliedBuffs.Contains(item);

    /// <summary>
    /// Expose applied buffs (read-only copy)
    /// </summary>
    public ItemData[] GetAppliedBuffs() => appliedBuffs.ToArray();

    // Helper - raise event
    private void RaiseStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }
}
