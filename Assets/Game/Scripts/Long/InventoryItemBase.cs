using UnityEngine;

public abstract class InventoryItemBase : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    [TextArea] public string description;
}
