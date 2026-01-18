// Item.cs
// ScriptableObject for general items (buffs) that can be permanent/stackable or timed.
// Create instances via Assets > Create > Inventory > Item
// Fields: name (from ScriptableObject), Icon for UI, isStackable, isTimed, duration (if timed).
// Implement Apply/Remove to modify PlayerStatManager stats (e.g., add flat bonuses).
// For timed items, Apply is called on add, Remove after duration.
// Multiplier is for stack count (e.g., +5 health * multiplier).

using UnityEngine;

// [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item", order = 3)]
public abstract class Item : ScriptableObject
{
    [Header("Display")]
    public Sprite Icon; // Icon for inventory slot

    [Header("Behavior")]
    public bool isStackable = true; // Can multiple instances stack? (e.g., +health potions)
    public bool isTimed = false;    // Is this a temporary buff?
    public float duration = 30f;    // Duration in seconds (if isTimed)

    // Apply effect to stats (multiplier = stack count added)
    public abstract void Apply(int multiplier = 1);

    // Remove effect from stats (multiplier = stack count removed)
    public abstract void Remove(int multiplier = 1);
}