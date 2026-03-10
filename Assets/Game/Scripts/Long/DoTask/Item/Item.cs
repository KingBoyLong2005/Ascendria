
using System;
using UnityEngine;

// [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item", order = 3)]
public abstract class Item : ScriptableObject
{
    [Header("Display")]
    public Sprite Icon; // Icon for inventory slot
    public String Name;
    public LevelManager.RarityTier rarity;

    [Header("Behavior")]
    public bool isStackable = true; // Can multiple instances stack? (e.g., +health potions)
    public bool isTimed = false;    // Is this a temporary buff?
    public float duration = 30f;    // Duration in seconds (if isTimed)

    // Apply effect to stats (multiplier = stack count added)
    public abstract void Apply(int multiplier = 1);

    // Remove effect from stats (multiplier = stack count removed)
    public abstract void Remove(int multiplier = 1);
}

