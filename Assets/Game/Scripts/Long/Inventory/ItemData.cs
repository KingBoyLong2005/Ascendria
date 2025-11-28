using UnityEngine;

public enum ItemType { Consumable, Equipment, Buff, Misc }
public enum BuffType { MaxHP, MoveSpeed, Luck, DamageFlat, DamageMultiplier, AttackSpeed, CritChance }

[CreateAssetMenu(fileName = "ItemData", menuName = "Game/Item")]
public class ItemData : InventoryItemBase
{
    public string itemName;
    public ItemType itemType = ItemType.Misc;
    // Buff properties (only valid when itemType == Buff)
    public BuffType buffType;
    public float buffValue;      // e.g., +20 HP, or 0.1 for +10% multiplier
    public string statKey;       // optional: directly target a custom stat key (overrides buffType mapping)
    public bool isPermanent = true; // if false, you will need to handle timed buff removal
}
