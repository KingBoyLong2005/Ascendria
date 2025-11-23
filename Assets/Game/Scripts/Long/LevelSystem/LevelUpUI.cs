using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class LevelUpUI : MonoBehaviour
{
    [Header("References")]
    public LevelSystem levelSystem; // assign in inspector
    public InventoryManager inventoryManager; // assign in inspector (or get Instance)
    public GameObject panel; // panel chứa options (set active false by default)
    public LevelUpOptionUI optionPrefab; // prefab for a single option (button + icon + text)
    public Transform optionsParent; // parent transform to instantiate options under
    public int optionCount = 3;

    [Header("Weapon pool (choose from this)")]
    public WeaponData[] weaponPool; // assign all possible weapons to offer

    List<LevelUpOptionUI> spawnedOptions = new List<LevelUpOptionUI>();
    bool isShowing = false;

    void Start()
    {
        if (levelSystem == null)
        {
            Debug.LogWarning("LevelUpUI: levelSystem not assigned. Trying to find one in scene.");
            levelSystem = FindFirstObjectByType<LevelSystem>();
        }
        if (inventoryManager == null && InventoryManager.Instance != null)
            inventoryManager = InventoryManager.Instance;

        if (levelSystem != null)
            levelSystem.OnLevelUp += OnLevelUp;

        if (panel != null) panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (levelSystem != null) levelSystem.OnLevelUp -= OnLevelUp;
    }

    void OnLevelUp(int newLevel)
    {
        ShowOptions();
    }

    public void ShowOptions()
    {
        if (isShowing) return;
        isShowing = true;
        // Disable player shooting input while choosing
        var player = FindFirstObjectByType<PlayerShooting>();
        if (player != null) player.enabled = false;
        var mouse = FindFirstObjectByType<TPCameraController>();
        if (mouse != null) mouse.isUIOpen = true;
        if (panel != null) panel.SetActive(true);

        // Clear existing
        foreach (var o in spawnedOptions) Destroy(o.gameObject);
        spawnedOptions.Clear();

        // Build list of candidate weapons
        List<WeaponData> candidates = new List<WeaponData>();
        if (weaponPool != null && weaponPool.Length > 0)
            candidates.AddRange(weaponPool);

        // Option: exclude already owned weapons to avoid duplicates
        if (inventoryManager != null)
        {
            candidates = candidates.Where(w => w != null && !inventoryManager.ownedWeapons.Contains(w)).ToList();
        }
        else
        {
            candidates = candidates.Where(w => w != null).ToList();
        }

        // If pool too small, fallback to weaponPool allowing duplicates
        List<WeaponData> choices = new List<WeaponData>();
        System.Random rnd = new System.Random();
        for (int i = 0; i < optionCount; i++)
        {
            if (candidates.Count == 0)
            {
                // fallback: choose from full pool (may include owned)
                if (weaponPool == null || weaponPool.Length == 0) break;
                choices.Add(weaponPool[rnd.Next(0, weaponPool.Length)]);
            }
            else
            {
                int idx = rnd.Next(0, candidates.Count);
                choices.Add(candidates[idx]);
                candidates.RemoveAt(idx);
            }
        }

        // instantiate option UI
        foreach (var w in choices)
        {
            var inst = Instantiate(optionPrefab, optionsParent);
            inst.Setup(w, OnOptionSelected);
            spawnedOptions.Add(inst);
        }
    }

    void OnOptionSelected(WeaponData selected)
    {
        // Add to inventory (will auto-equip if InventoryManager.AddWeapon logic permits)
        if (selected != null && inventoryManager != null)
        {
            inventoryManager.AddWeapon(selected, equipIfSpace: true);
        }

        CloseOptions();
    }

    public void CloseOptions()
    {
        if (!isShowing) return;
        isShowing = false;
        if (panel != null) panel.SetActive(false);

        // re-enable player shooting
        var player = FindFirstObjectByType<PlayerShooting>();
        if (player != null) player.enabled = true;
        var mouse = FindFirstObjectByType<TPCameraController>();
        if (mouse != null) mouse.isUIOpen = false;
    }
}
