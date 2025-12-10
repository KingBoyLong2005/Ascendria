using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// Inventory UI controller (updated to use a single slot prefab for both weapons and items).
/// - Set "slotPrefab" to a prefab that has InventorySlotUI.
/// - Keep separate parents (weaponsParent / itemsParent) if you want grouping in UI.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Panel & Input")]
    public GameObject inventoryPanel;   // root panel (set inactive by default)
    public KeyCode toggleKey = KeyCode.I;
    public bool unlockCursorWhenOpen = true;
    public bool pauseTimeWhenOpen = false;

    [Header("Slots UI")]
    public Transform weaponsParent;
    public Transform itemsParent;
    public GameObject slotPrefab; // unified prefab (InventorySlotUI)

    // internal lists to manage spawned UI elements
    List<GameObject> spawnedSlots = new List<GameObject>();

    bool isOpen = false;

    void Awake()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged +=(s,e) => RefreshAll();
            InventoryManager.Instance.OnActiveWeaponsChanged += (s,e) => RefreshAll();
        }
        RefreshAll();
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= (s,e) => RefreshAll();
            InventoryManager.Instance.OnActiveWeaponsChanged -= (s,e) => RefreshAll();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) ToggleInventory();
        if (isOpen && Input.GetKeyDown(KeyCode.Escape)) CloseInventory();
    }

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
        RefreshAll();

        if (unlockCursorWhenOpen) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
        if (pauseTimeWhenOpen) Time.timeScale = 0f;

        var es = EventSystem.current;
        if (es != null && spawnedSlots.Count > 0) es.SetSelectedGameObject(spawnedSlots[0]);
    }

    public void CloseInventory()
    {
        if (inventoryPanel == null) return;
        isOpen = false;
        inventoryPanel.SetActive(false);

        if (unlockCursorWhenOpen) { Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; }
        if (pauseTimeWhenOpen) Time.timeScale = 1f;

        var es = EventSystem.current;
        if (es != null) es.SetSelectedGameObject(null);
    }

    #region Refresh / Populate

    public void RefreshAll()
    {
        // clear existing slots
        foreach (var go in spawnedSlots) Destroy(go);
        spawnedSlots.Clear();

        if (InventoryManager.Instance == null || slotPrefab == null) return;

        // Weapons (owned)
        if (weaponsParent != null)
        {
            foreach (var w in InventoryManager.Instance.ownedWeapons)
            {
                if (w == null) continue;
                GameObject go = Instantiate(slotPrefab, weaponsParent);
                var slot = go.GetComponent<InventorySlotUI>();
                if (slot != null) slot.Bind(w);
                spawnedSlots.Add(go);
            }
        }

        // Items
        // if (itemsParent != null)
        // {
        //     foreach (var it in InventoryManager.Instance.items)
        //     {
        //         if (it == null) continue;
        //         GameObject go = Instantiate(slotPrefab, itemsParent);
        //         var slot = go.GetComponent<InventorySlotUI>();
        //         if (slot != null) slot.Bind(it);
        //         spawnedSlots.Add(go);
        //     }
        // }

        // ensure equipped visuals up to date
        foreach (var go in spawnedSlots)
        {
            var slot = go.GetComponent<InventorySlotUI>();
            if (slot != null) slot.UpdateEquippedVisual();
        }
    }

    #endregion
}
