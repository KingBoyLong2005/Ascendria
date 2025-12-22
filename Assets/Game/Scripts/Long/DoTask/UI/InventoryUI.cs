using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject inventoryPanel;

    [Header("Slots UI")]
    public Transform weaponsParent;
    public Transform buffsParent;
    public Transform itemsParent;
    public GameObject slotPrefab;

    readonly List<GameObject> spawnedSlots = new();
    bool isOpen;

    void Awake()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        EnsureLayout(weaponsParent);
        EnsureLayout(buffsParent);
        if (itemsParent != null) EnsureLayout(itemsParent);
    }

    #region Public API (ONLY UIManager gọi)

    public void Open()
    {
        GameManager.Instance.PauseGame();
        FindFirstObjectByType<TPCameraController>().isUIOpen = true;
        if (isOpen) return;
        isOpen = true;

        inventoryPanel.SetActive(true);
        RefreshAll();
    }

    public void Close()
    {
        GameManager.Instance.ResumeGame();
        FindFirstObjectByType<TPCameraController>().isUIOpen = false;
        if (!isOpen) return;
        isOpen = false;

        inventoryPanel.SetActive(false);
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void RefreshAll()
    {
        foreach (var go in spawnedSlots)
            Destroy(go);
        spawnedSlots.Clear();

        if (InventoryManager.Instance == null || slotPrefab == null)
            return;

        SpawnWeapons();
        SpawnBuffs();
        SpawnItems();
    }

    #endregion

    #region Internal

    void SpawnWeapons()
    {
        if (weaponsParent == null) return;

        foreach (var w in InventoryManager.Instance.ownedWeapons)
        {
            if (w == null) continue;
            var go = Instantiate(slotPrefab, weaponsParent);
            go.GetComponent<InventorySlotUI>()?.Bind(w);
            spawnedSlots.Add(go);
        }
    }

    void SpawnBuffs()
    {
        if (buffsParent == null) return;

        foreach (var bb in InventoryManager.Instance.ownedBookBuffs)
        {
            if (bb == null) continue;
            var go = Instantiate(slotPrefab, buffsParent);
            go.GetComponent<InventorySlotUI>()?.Bind(bb);
            spawnedSlots.Add(go);
        }
    }

    void SpawnItems()
    {
        if (itemsParent == null) return;

        foreach (var kv in InventoryManager.Instance.ownedItems)
        {
            var go = Instantiate(slotPrefab, itemsParent);
            go.GetComponent<InventorySlotUI>()?.Bind(kv.Key, kv.Value);
            spawnedSlots.Add(go);
        }
    }

    void EnsureLayout(Transform parent)
    {
        if (parent == null) return;
        if (parent.GetComponent<GridLayoutGroup>() != null) return;

        var grid = parent.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(100, 100);
        grid.spacing = new Vector2(10, 10);
        grid.childAlignment = TextAnchor.UpperLeft;

        var fitter = parent.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    #endregion
}

// // InventoryUI.cs (ensure parents have layout to prevent overlap; assuming GridLayoutGroup is added in Editor)
// using UnityEngine;
// using System.Collections.Generic;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using Mono.CSharp;
// using System; // For GridLayoutGroup

// /// <summary>
// /// Inventory UI controller (updated to use a single slot prefab for both weapons and items).
// /// - Set "slotPrefab" to a prefab that has InventorySlotUI.
// /// - Keep separate parents (weaponsParent / itemsParent) if you want grouping in UI.
// /// - Added buffsParent for BookBuffs.
// /// - Assume weaponsParent and buffsParent have GridLayoutGroup component for auto-arrangement.
// /// </summary>
// public class InventoryUI : MonoBehaviour
// {
//     [Header("Panel & Input")]
//     public GameObject inventoryPanel;   // root panel (set inactive by default)
//     public KeyCode toggleKey = KeyCode.I;
//     public bool unlockCursorWhenOpen = true;
//     public bool pauseTimeWhenOpen = false;

//     [Header("Slots UI")]
//     public Transform weaponsParent;
//     public Transform buffsParent; // New: Parent for buff slots
//     public Transform itemsParent; // Existing, but commented in original
//     public GameObject slotPrefab; // unified prefab (InventorySlotUI)

//     // internal lists to manage spawned UI elements
//     List<GameObject> spawnedSlots = new List<GameObject>();

//     bool isOpen = false;

//     void Awake()
//     {
//         if (inventoryPanel != null) inventoryPanel.SetActive(false);

//         // Ensure parents have GridLayoutGroup for auto-sorting/arrangement (add if missing)
//         EnsureLayout(weaponsParent);
//         EnsureLayout(buffsParent);
//         if (itemsParent != null) EnsureLayout(itemsParent);
//     }

//     private void EnsureLayout(Transform parent)
//     {
//         if (parent == null) return;
//         if (parent.GetComponent<GridLayoutGroup>() == null)
//         {
//             var grid = parent.gameObject.AddComponent<GridLayoutGroup>();
//             grid.childAlignment = TextAnchor.UpperLeft;
//             grid.spacing = new Vector2(10f, 10f); // Adjust spacing as needed
//             grid.cellSize = new Vector2(100f, 100f); // Adjust cell size based on slot prefab
//             // Add ContentSizeFitter if needed for dynamic sizing
//             var fitter = parent.gameObject.AddComponent<ContentSizeFitter>();
//             fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
//             fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
//         }
//     }

//     // void Start()
//     // {
//     //     // if (InventoryManager.Instance != null)
//     //     // {
//     //     //     // InventoryManager.Instance.OnInventoryChanged +=(s,e) => RefreshAll();
//     //     //     // InventoryManager.Instance.OnActiveWeaponsChanged += (s,e) => RefreshAll();
//     //     //     // InventoryManager.Instance.OnActiveBookBuffsChanged += (s,e) => RefreshAll(); // Added for buffs
//     //     //     // InventoryManager.Instance.OnActiveItemsChanged += (s, e) => RefreshAll();
//     //     // }
//     //     RefreshAll();
//     // }

//     // void OnDestroy()
//     // {
//     //     if (InventoryManager.Instance != null)
//     //     {
//     //         // InventoryManager.Instance.OnInventoryChanged -= (s,e) => RefreshAll();
//     //         // InventoryManager.Instance.OnActiveWeaponsChanged -= (s,e) => RefreshAll();
//     //         // InventoryManager.Instance.OnActiveBookBuffsChanged -= (s,e) => RefreshAll();
//     //         // InventoryManager.Instance.OnActiveItemsChanged -= (s, e) => RefreshAll();
//     //     }
//     // }

//     // void Update()
//     // {
//     //     if (Input.GetKeyDown(toggleKey)) ToggleInventory();
//     //     if (isOpen && Input.GetKeyDown(KeyCode.Escape)) CloseInventory();
//     // }

//     public void ToggleInventory()
//     {
//         if (isOpen) CloseInventory();
//         else OpenInventory();
//     }

//     public void OpenInventory()
//     {
//         FindFirstObjectByType<TPCameraController>().isUIOpen = true;
//         GameManager.Instance.PauseGame();
//         if (inventoryPanel == null) return;
//         isOpen = true;
//         inventoryPanel.SetActive(true);
//         RefreshAll();

//         if (unlockCursorWhenOpen) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }

//         var es = EventSystem.current;
//         if (es != null && spawnedSlots.Count > 0) es.SetSelectedGameObject(spawnedSlots[0]);
//     }

//     public void CloseInventory()
//     {
//         FindFirstObjectByType<TPCameraController>().isUIOpen = false;
//         GameManager.Instance.ResumeGame();
//         if (inventoryPanel == null) return;
//         isOpen = false;
//         inventoryPanel.SetActive(false);

//         if (unlockCursorWhenOpen) { Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; }

//         var es = EventSystem.current;
//         if (es != null) es.SetSelectedGameObject(null);
//     }

//     #region Refresh / Populate

//     public void RefreshAll()
//     {
//         // clear existing slots
//         foreach (var go in spawnedSlots) Destroy(go);
//         spawnedSlots.Clear();

//         if (InventoryManager.Instance == null || slotPrefab == null) return;

//         // Weapons (owned, already sorted in InventoryManager)
//         if (weaponsParent != null)
//         {
//             foreach (var w in InventoryManager.Instance.ownedWeapons)
//             {
//                 if (w == null) continue;
//                 GameObject go = Instantiate(slotPrefab, weaponsParent);
//                 var slot = go.GetComponent<InventorySlotUI>();
//                 if (slot != null) slot.Bind(w);
//                 spawnedSlots.Add(go);
//             }
//         }

//         // BookBuffs (owned, already sorted in InventoryManager)
//         if (buffsParent != null)
//         {
//             foreach (var bb in InventoryManager.Instance.ownedBookBuffs)
//             {
//                 if (bb == null) continue;
//                 GameObject go = Instantiate(slotPrefab, buffsParent);
//                 var slot = go.GetComponent<InventorySlotUI>();
//                 if (slot != null) slot.Bind(bb);
//                 spawnedSlots.Add(go);
//             }
//         }

//         if (itemsParent != null && ItemManager.Instance != null)
//         {
//             foreach (var kv in InventoryManager.Instance.ownedItems) // Access via property or make public/getter
//             {
//                 if (kv.Key == null) continue;
//                 GameObject go = Instantiate(slotPrefab, itemsParent);
//                 var slot = go.GetComponent<InventorySlotUI>();
//                 if (slot != null) slot.Bind(kv.Key, kv.Value);
//                 spawnedSlots.Add(go);
//             }
//         }

//         // ensure equipped visuals up to date
//         foreach (var go in spawnedSlots)
//         {
//             var slot = go.GetComponent<InventorySlotUI>();
//             if (slot != null) slot.UpdateEquippedVisual();
//         }

//         // Force layout rebuild to ensure no overlap
//         LayoutRebuilder.ForceRebuildLayoutImmediate(weaponsParent as RectTransform);
//         LayoutRebuilder.ForceRebuildLayoutImmediate(buffsParent as RectTransform);
//         if (itemsParent != null) LayoutRebuilder.ForceRebuildLayoutImmediate(itemsParent as RectTransform);
//     }

//     #endregion
// }