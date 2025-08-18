using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string description;
    public float buffValue;  // Có thể là +HP, +Speed,...
    public float debuffValue; // Nếu có
    public ItemType itemType; // Enum: Buff, Debuff, Upgrade, Quest...
}

public enum ItemType
{
    Weapon,
    Buff,
    Debuff,
    Upgrade,
    Quest
}
