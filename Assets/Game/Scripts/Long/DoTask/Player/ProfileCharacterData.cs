using UnityEngine;

[CreateAssetMenu(menuName = "Character/Profile Character Data")]
public class ProfileCharacterData : ScriptableObject
{
    [Header("Identity")]
    // public string characterId;
    public string displayName;

    [Header("Stats")]
    public float maxHP;
    public float CharacterAttack;
    public float Armor;
    public float MoveSpeed;

    [Tooltip("Increase drop rate of Silver, increase chance of higher rarity options when level up")]
    public float Luck;
    [Tooltip("Increase the amount of Gold and Silver that drop")]
    public float Wealth;
    [Tooltip("Increase the amount of each XPGem")]
    public float Wise;

    [Header("Visual")]
    public GameObject modelPrefab;
    public RuntimeAnimatorController animatorController;

    [Header("Combat")]
    public Weapon startingWeapon;

    // [Header("UI")]
    // public Sprite portrait;
}
