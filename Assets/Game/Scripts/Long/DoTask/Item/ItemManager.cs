// ItemManager.cs
// This manager handles the effects of items (apply/remove buffs, timers).
// Assumes items are auto-applied when equipped in InventoryManager.
// Integrates with InventoryManager for owned/active updates.
// Item is a ScriptableObject with fields: name, Icon (Sprite), isStackable (bool), isTimed (bool), duration (float),
// and methods: Apply(PlayerStatManager stats, int multiplier), Remove(PlayerStatManager stats, int multiplier).

using UnityEngine;
using System;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("All Possible Items")]
    private ItemDatabase allPossibleItems; // List to pick random from (populate in Inspector or code)

    private List<TimedBuff> activeTimedBuffs = new List<TimedBuff>(); // For managing timed buffs

    private PlayerStatManager stats;

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
        // Wait for InventoryManager to be ready, similar to BookBuffManager
        if (InventoryManager.Instance != null)
        {
            HandleInventoryReady(null, EventArgs.Empty);
        }
        else
        {
            InventoryManager.Instance.OnInventoryReady += HandleInventoryReady;
        }
        allPossibleItems = ItemDatabase.Instance;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryReady -= HandleInventoryReady;
        }
    }

    private void HandleInventoryReady(object sender, EventArgs e)
    {
        stats = PlayerStatManager.Instance;

        // Reapply all owned permanent items on start
        ReapplyAll();
    }

    private void Update()
    {
        // Tick timers for timed buffs
        for (int i = activeTimedBuffs.Count - 1; i >= 0; i--)
        {
            activeTimedBuffs[i].Tick(Time.deltaTime);
            if (activeTimedBuffs[i].IsExpired)
            {
                var tb = activeTimedBuffs[i];
                RemoveItemEffect(tb.item, tb.multiplier);
                InventoryManager.Instance.RemoveItem(tb.item, tb.multiplier);
                activeTimedBuffs.RemoveAt(i);
            }
        }

    }

    // Apply effect for added count
    public void ApplyItemEffect(Item item, int addedMultiplier)
    {
        if (stats == null || item == null || addedMultiplier <= 0) return;

        item.Apply(stats, addedMultiplier);

        if (item.isTimed)
        {
            // Add a new timer entry for the added batch
            activeTimedBuffs.Add(new TimedBuff(item, addedMultiplier, item.duration));
        }
    }

    // Remove effect for specified count (e.g., expired batch or manual remove)
    public void RemoveItemEffect(Item item, int multiplier)
    {
        if (stats == null || item == null || multiplier <= 0) return;

        item.Remove(stats, multiplier);
    }

    // Remove all effects for a specific item (e.g., for unequip)
    public void RemoveAllForItem(Item item)
    {
        if (stats == null || item == null) return;

        int totalMultiplier = 0;

        if (item.isTimed)
        {
            for (int i = activeTimedBuffs.Count - 1; i >= 0; i--)
            {
                if (activeTimedBuffs[i].item == item)
                {
                    totalMultiplier += activeTimedBuffs[i].multiplier;
                    activeTimedBuffs.RemoveAt(i);
                }
            }
        }
        else
        {
            var inv = InventoryManager.Instance;
            if (inv != null && inv.ownedItems.TryGetValue(item, out var c))
            {
                totalMultiplier = c;
            }
        }

        if (totalMultiplier > 0)
        {
            item.Remove(stats, totalMultiplier);
        }
    }

    // Reapply all permanent items (called on start or reload)
    public void ReapplyAll()
    {
        activeTimedBuffs.Clear(); // Timed expire on reload

        var inv = InventoryManager.Instance;
        if (inv == null || stats == null) return;

        foreach (var item in inv.activeItems)
        {
            if (!item.isTimed && inv.ownedItems.TryGetValue(item, out var count))
            {
                item.Apply(stats, count);
            }
        }
    }

    // Get a random item from the list
    public Item GetRandomItem()
    {
        var db = ItemDatabase.Instance;
        if (db == null || db.item == null || db.item.Length == 0) return null;
        return db.item[UnityEngine.Random.Range(0, db.item.Length)];
    }
}

// Helper class for timed buffs
[Serializable]
public class TimedBuff
{
    public Item item;
    public int multiplier;
    public float remainingTime;

    public bool IsExpired => remainingTime <= 0;

    public TimedBuff(Item i, int m, float d)
    {
        item = i;
        multiplier = m;
        remainingTime = d;
    }

    public void Tick(float delta)
    {
        if (remainingTime > 0)
        {
            remainingTime -= delta;
        }
    }
}