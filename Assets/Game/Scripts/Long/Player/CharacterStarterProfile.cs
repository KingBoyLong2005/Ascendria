using UnityEngine;

[DisallowMultipleComponent]
public class CharacterStarterProfile : MonoBehaviour
{   
    [Header("Base Stats")]
    public float maxHP = 100f;
    public float moveSpeed = 5f;
    public float luck = 0f;

    [Header("Expandable Custom Stats")]
    public StatEntry[] extraStats;

    [System.Serializable]
    public struct StatEntry
    {
        public string key;
        public float value;
    }

    [Tooltip("Assign the weapon-only profile for this character")]
    public CharacterWeaponProfile profile;

    [Tooltip("If true, tries to add starting weapons on Start()")]
    public bool addOnStart = true;

    void Start()
    {
        if (!addOnStart) return;

        if (profile == null)
        {
            Debug.LogWarning($"CharacterStarterWeapons on {gameObject.name} has no profile assigned.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager.Instance is null. Make sure InventoryManager exists in the scene before the player starts.");
            return;
        }

        foreach (var w in profile.startingWeapons)
        {
            if (w == null) continue;
            InventoryManager.Instance.AddWeapon(w, equipIfSpace: profile.autoEquipOnStart);
        }
    }

    // helper to add profile dynamically at runtime (e.g., after spawn)
    public void ApplyProfile(CharacterWeaponProfile p)
    {
        profile = p;
        if (profile == null) return;
        if (InventoryManager.Instance == null) return;

        foreach (var w in profile.startingWeapons)
        {
            if (w == null) continue;
            InventoryManager.Instance.AddWeapon(w, equipIfSpace: profile.autoEquipOnStart);
        }
    }
}
