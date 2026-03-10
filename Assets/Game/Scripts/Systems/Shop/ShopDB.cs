using UnityEngine;

[CreateAssetMenu(menuName = "ShopData/ShopDB")]
public class ShopDB : ScriptableObject
{
    public ShopItemDefinition[] items;

    public ShopItemDefinition GetItem(string id)
    {
        foreach (var item in items)
        {
            if (item.id == id)
                return item;
        }
        return null;
    }
}
