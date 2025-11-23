using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapon")]
public class WeaponData : InventoryItemBase
{
    public WeaponTypeLong weaponType = WeaponTypeLong.Projectile;

    public float baseDamage = 10f;
    public float attackRate = 1f;
    public float range = 5f;

    [Header("Prefab đại diện")]
    public GameObject prefab;

    [Header("Legacy (tự sync sang displayName)")]
    public string weaponName;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(weaponName))
            displayName = weaponName;
    }
}


public enum WeaponTypeLong
{
    Projectile,
    Melee,
    Throw,
    AoE
}
