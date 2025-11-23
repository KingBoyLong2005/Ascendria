using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
public class ItemData : InventoryItemBase
{
    public string itemName;
    public ItemType itemType;

    [Header("Optional Buff/Value")]
    public float buffValue;
    public float debuffValue;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(itemName))
            displayName = itemName;
    }
}

public enum ItemType
{
    Weapon,
    Buff,
    Debuff,
    Upgrade,
    Quest
}
