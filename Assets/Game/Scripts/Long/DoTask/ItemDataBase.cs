using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    private static ItemDatabase _instance;

    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                if (_instance == null)
                    Debug.LogError("ItemDatabase.asset not found in Resources!");
            }
            return _instance;
        }
    }

    [Header("Weapons")]
    public Item[] item;
}
