using UnityEngine;

[CreateAssetMenu(fileName = "CharacterWeaponsProfile", menuName = "Characters/Weapon Profile")]
public class CharacterWeaponProfile : ScriptableObject
{
    [Header("Starting weapons for this character")]
    public WeaponData[] startingWeapons;

    [Tooltip("If true, weapons will be auto-equipped into activeWeapons when added (until maxActiveWeapons)")]
    public bool autoEquipOnStart = true;
}
