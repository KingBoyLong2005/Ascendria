using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "Characters/Character Profile")]
public class CharacterStaterProfile : ScriptableObject
{
    [Header("Base Stats")]
    public float maxHP = 100f;
    public float moveSpeed = 5f;
    public float luck = 0f;

    [Header("Extra / Custom Stats (key/value)")]
    public ExtraStat[] extraStats;

    [Header("Starting weapons (can use CharacterWeaponProfile or list weapons directly)")]
    public WeaponData startingWeapons;

    [System.Serializable]
    public struct ExtraStat
    {
        public string key;
        public float value;
    }
}
