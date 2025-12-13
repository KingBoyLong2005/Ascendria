using UnityEngine;

[CreateAssetMenu(menuName = "Game/Upgrade Database")]
public class UpgradeDatabase : ScriptableObject
{
    private static UpgradeDatabase _instance;

    public static UpgradeDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<UpgradeDatabase>("UpgradeDatabase");
                if (_instance == null)
                    Debug.LogError("UpgradeDatabase.asset not found in Resources!");
            }
            return _instance;
        }
    }

    [Header("Weapons")]
    public Weapon[] allWeapons;

    [Header("Buffs")]
    public BookBuff[] allBuffs;
}
