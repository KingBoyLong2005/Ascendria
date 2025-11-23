using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// Inventory UI controller:
/// - Populate weapon slots and item slots (prefab-driven)
/// - Toggle inventory panel with I, close with Esc or I
/// - When opened it shows cursor and pauses input optionally
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Panel & Input")]
    public GameObject inventoryPanel;   // root panel (set inactive by default)
    public KeyCode toggleKey = KeyCode.I;
    public bool unlockCursorWhenOpen = true;
    public bool pauseTimeWhenOpen = false;

    [Header("Weapons UI")]
    public Transform weaponsParent;
    public GameObject weaponSlotPrefab;

    [Header("Items UI")]
    public Transform itemsParent;
    public GameObject itemSlotPrefab;

    // internal lists to manage spawned UI elements
    List<GameObject> spawnedWeaponSlots = new List<GameObject>();
    List<GameObject> spawnedItemSlots = new List<GameObject>();

    bool isOpen = false;

    void Awake()
    {
        // ensure panel start state
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    void Start()
    {
        // subscribe to inventory events so UI updates automatically
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnChanged += RefreshAll;
            InventoryManager.Instance.OnWeaponsChanged += RefreshWeapons;
        }
        // populate once in case there are initial items
        RefreshAll();
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnChanged -= RefreshAll;
            InventoryManager.Instance.OnWeaponsChanged -= RefreshWeapons;
        }
    }

    void Update()
    {
        // toggle inventory
        if (Input.GetKeyDown(toggleKey))
        {
            // inputAxisController.enabled = false;
            ToggleInventory();
        }

        // close with Escape
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            // var MouseActive = FindFirstObjectByType<TPCameraController>();
            // MouseActive.isUIOpen = false;
            CloseInventory();
        }
    }

    #region Toggle / Open / Close

    public void ToggleInventory()
    {
        if (isOpen) CloseInventory();
        else OpenInventory();
    }

    public void OpenInventory()
    {
        if (inventoryPanel == null) return;

        isOpen = true;
        inventoryPanel.SetActive(true);
        RefreshAll(); // ensure latest content

        if (unlockCursorWhenOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (pauseTimeWhenOpen)
        {
            Time.timeScale = 0f;
        }

        // Optionally set first selected UI for gamepad/keyboard navigation
        var es = EventSystem.current;
        if (es != null)
        {
            // try to select first weapon slot if exists
            if (spawnedWeaponSlots.Count > 0)
                es.SetSelectedGameObject(spawnedWeaponSlots[0]);
            else if (spawnedItemSlots.Count > 0)
                es.SetSelectedGameObject(spawnedItemSlots[0]);
        }
    }

    public void CloseInventory()
    {
        if (inventoryPanel == null) return;

        isOpen = false;
        inventoryPanel.SetActive(false);

        if (unlockCursorWhenOpen)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (pauseTimeWhenOpen)
        {
            Time.timeScale = 1f;
        }

        // clear UI selection
        var es = EventSystem.current;
        if (es != null) es.SetSelectedGameObject(null);
    }

    #endregion

    #region Refresh / Populate

    public void RefreshAll()
    {
        RefreshWeapons();
        RefreshItems();
    }

    public void RefreshWeapons()
    {
        // clear existing slots
        foreach (var go in spawnedWeaponSlots) Destroy(go);
        spawnedWeaponSlots.Clear();

        if (InventoryManager.Instance == null || weaponSlotPrefab == null || weaponsParent == null) return;

        foreach (var w in InventoryManager.Instance.ownedWeapons)
        {
            if (w == null) continue;
            GameObject go = Instantiate(weaponSlotPrefab, weaponsParent);
            var slot = go.GetComponent<WeaponSlotUI>();
            if (slot != null) slot.Bind(w);
            spawnedWeaponSlots.Add(go);
        }

        // update equipped visuals (in case equip state changed)
        foreach (var go in spawnedWeaponSlots)
        {
            var slot = go.GetComponent<WeaponSlotUI>();
            if (slot != null) slot.UpdateEquippedVisual();
        }
    }

    public void RefreshItems()
    {
        // clear existing slots
        foreach (var go in spawnedItemSlots) Destroy(go);
        spawnedItemSlots.Clear();

        if (InventoryManager.Instance == null || itemSlotPrefab == null || itemsParent == null) return;

        foreach (var it in InventoryManager.Instance.items)
        {
            if (it == null) continue;
            GameObject go = Instantiate(itemSlotPrefab, itemsParent);
            var slot = go.GetComponent<ItemSlotUI>();
            if (slot != null) slot.Bind(it);
            spawnedItemSlots.Add(go);
        }
    }

    #endregion
}
