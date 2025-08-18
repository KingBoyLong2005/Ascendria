//using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerWeaponSelector : MonoBehaviour
{
    [SerializeField] private WeaponType Weapon;
    [SerializeField] private Transform WeaponParent;
    [SerializeField] private List<WeaponScriptableObject> WeaponList;
    //[SerializeField] private PlayerIK InverseKinematic;

    [Space]
    [Header("Runtime Filled")]
    public WeaponScriptableObject ActiveWeapon;

    private void Start()
    {
        WeaponScriptableObject weapon = WeaponList.Find(weapon => weapon.Type == Weapon);
        if (weapon == null) 
        {
            Debug.LogError($"No WeaponScriptObject found for WeaponType: {weapon}");
            return;
        }

        ActiveWeapon = weapon;

        weapon.Spawn(WeaponParent, this);
    }
}
