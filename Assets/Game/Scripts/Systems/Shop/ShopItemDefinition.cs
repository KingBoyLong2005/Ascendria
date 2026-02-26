using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ShopData/ShopItemDefinition")]
public class ShopItemDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public string description;

    [Header("Level Data")]
    public ShopItemLevelData[] levels;

    public int maxLevel => levels != null ? levels.Length : 0;

    public ShopItemLevelData GetLevelData(int level)
    {
        if (levels == null || levels.Length == 0)
            return null;

        int index = Mathf.Clamp(level - 1, 0, levels.Length - 1);
        return levels[index];
    }
    
}
