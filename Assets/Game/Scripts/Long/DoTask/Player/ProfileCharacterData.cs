using UnityEngine;

[CreateAssetMenu(menuName = "Character/Profile Character Data")]
public class ProfileCharacterData : ScriptableObject
{
    [Header("Identity")]
    // public string characterId;
    public string displayName;

    [Header("Stats")]
    public int maxHP;
    public float moveSpeed;
    public float attackSpeed;

    [Header("Visual")]
    public GameObject modelPrefab;
    public RuntimeAnimatorController animatorController;

    [Header("Combat")]
    public Weapon startingWeapon;

    // [Header("UI")]
    // public Sprite portrait;
}
