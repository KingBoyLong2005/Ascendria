using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// InventoryUI được tổ chức thành 3 hàng ngang:
/// - Hàng 1 (trên cùng): Weapons - Mỗi weapon chỉ hiện 1 lần
/// - Hàng 2 (giữa): Buffs - Mỗi buff chỉ hiện 1 lần  
/// - Hàng 3 (dưới): Items - Hiển thị số lượng stackable
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject inventoryPanel;

    [Header("Sections - 3 Rows")]
    [Tooltip("Hàng 1 - Weapons (trên cùng)")]
    public Transform weaponsParent;
    
    [Tooltip("Hàng 2 - Buffs (giữa)")]
    public Transform buffsParent;
    
    [Tooltip("Hàng 3 - Items (dưới)")]
    public Transform itemsParent;
    
    [Header("Slot Prefab")]
    public GameObject slotPrefab;

    [Header("Layout Settings")]
    [Tooltip("Kích thước mỗi ô slot")]
    public Vector2 cellSize = new Vector2(100, 100);
    
    [Tooltip("Khoảng cách giữa các slot")]
    public Vector2 spacing = new Vector2(10, 10);
    
    [Tooltip("Số slot tối đa trên 1 hàng ngang")]
    public int maxSlotsPerRow = 6;

    readonly List<GameObject> spawnedSlots = new();
    public bool isOpen;

    void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            Debug.Log("<color=magenta>[InventoryUI]</color> Awake: inventoryPanel set to FALSE");
        }
        else
        {
            Debug.LogError("<color=red>[InventoryUI]</color> inventoryPanel is NULL in Awake!");
        }

        // Setup layout cho từng hàng
        SetupRowLayout(weaponsParent, "Weapons Row");
        SetupRowLayout(buffsParent, "Buffs Row");
        SetupRowLayout(itemsParent, "Items Row");
    }

    #region Public API

    public void Open()
    {
        FindFirstObjectByType<TPCameraController>().isUIOpen = true;
        GameManager.Instance.PauseGame();
        // if (cameraController != null)
        //     cameraController.isUIOpen = true;
            
        if (isOpen) return;
        isOpen = true;

        inventoryPanel.SetActive(true);
        RefreshAll();
        
        Debug.Log("<color=cyan>[InventoryUI]</color> Inventory opened");
    }

    public void Close()
    {
        FindFirstObjectByType<TPCameraController>().isUIOpen = false;
        GameManager.Instance.ResumeGame();
        // if (cameraController != null)
        //     cameraController.isUIOpen = false;
            
        if (!isOpen) return;
        isOpen = false;

        inventoryPanel.SetActive(false);
        
        Debug.Log("<color=cyan>[InventoryUI]</color> Inventory closed");
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void RefreshAll()
    {

        // Xóa tất cả slots hiện tại
        foreach (var go in spawnedSlots)
            Destroy(go);
        spawnedSlots.Clear();

        if (InventoryManager.Instance == null || slotPrefab == null)
        {
            Debug.LogWarning("<color=orange>[InventoryUI]</color> InventoryManager or slotPrefab is null!");
            return;
        }

        // Spawn theo thứ tự: Hàng 1 → Hàng 2 → Hàng 3
        SpawnWeaponsRow();
        SpawnBuffsRow();
        SpawnItemsRow();
        
        Debug.Log($"<color=green>[InventoryUI]</color> Refreshed inventory UI - " +
                  $"Weapons: {InventoryManager.Instance.ownedWeapons.Count}, " +
                  $"Buffs: {InventoryManager.Instance.ownedBookBuffs.Count}, " +
                  $"Items: {InventoryManager.Instance.ownedItems.Count}");
    }

    #endregion

    #region Spawn Methods

    /// <summary>
    /// HÀNG 1 - Weapons (mỗi weapon chỉ hiện 1 lần)
    /// </summary>
    void SpawnWeaponsRow()
    {
        if (weaponsParent == null)
        {
            Debug.LogWarning("<color=orange>[InventoryUI]</color> weaponsParent is null!");
            return;
        }

        int count = 0;
        foreach (var w in InventoryManager.Instance.ownedWeapons)
        {
            if (w == null) continue;
            
            var go = Instantiate(slotPrefab, weaponsParent);
            var slot = go.GetComponent<InventorySlotUI>();
            
            if (slot != null)
            {
                slot.Bind(w);
                count++;
            }
            else
            {
                Debug.LogError("<color=red>[InventoryUI]</color> Slot prefab missing InventorySlotUI component!");
                Destroy(go);
                continue;
            }
                
            spawnedSlots.Add(go);
        }
        
        Debug.Log($"<color=cyan>[InventoryUI]</color> Hàng 1 - Spawned {count} weapons");
    }

    /// <summary>
    /// HÀNG 2 - Buffs (mỗi buff chỉ hiện 1 lần)
    /// </summary>
    void SpawnBuffsRow()
    {
        if (buffsParent == null)
        {
            Debug.LogWarning("<color=orange>[InventoryUI]</color> buffsParent is null!");
            return;
        }

        int count = 0;
        foreach (var bb in InventoryManager.Instance.ownedBookBuffs)
        {
            if (bb == null) continue;
            
            var go = Instantiate(slotPrefab, buffsParent);
            var slot = go.GetComponent<InventorySlotUI>();
            
            if (slot != null)
            {
                slot.Bind(bb);
                count++;
            }
            else
            {
                Debug.LogError("<color=red>[InventoryUI]</color> Slot prefab missing InventorySlotUI component!");
                Destroy(go);
                continue;
            }
                
            spawnedSlots.Add(go);
        }
        
        Debug.Log($"<color=cyan>[InventoryUI]</color> Hàng 2 - Spawned {count} buffs");
    }

    /// <summary>
    /// HÀNG 3 - Items (hiển thị số lượng nếu stackable)
    /// </summary>
    void SpawnItemsRow()
    {
        if (itemsParent == null)
        {
            Debug.LogWarning("<color=orange>[InventoryUI]</color> itemsParent is null!");
            return;
        }

        int count = 0;
        foreach (var kv in InventoryManager.Instance.ownedItems)
        {
            if (kv.Key == null) continue;
            
            var go = Instantiate(slotPrefab, itemsParent);
            var slot = go.GetComponent<InventorySlotUI>();
            
            if (slot != null)
            {
                slot.Bind(kv.Key, kv.Value);
                count++;
            }
            else
            {
                Debug.LogError("<color=red>[InventoryUI]</color> Slot prefab missing InventorySlotUI component!");
                Destroy(go);
                continue;
            }
                
            spawnedSlots.Add(go);
        }
        
        Debug.Log($"<color=cyan>[InventoryUI]</color> Hàng 3 - Spawned {count} item types");
    }

    #endregion

    #region Layout Setup

    /// <summary>
    /// Thiết lập GridLayoutGroup cho mỗi hàng
    /// - Sắp xếp theo chiều ngang (left to right)
    /// - Tự động xuống hàng khi đầy maxSlotsPerRow
    /// - Kích thước tự động điều chỉnh theo nội dung
    /// </summary>
    void SetupRowLayout(Transform parent, string rowName)
    {
        if (parent == null) return;

        // // Xóa các layout cũ nếu có
        // var oldGrid = parent.GetComponent<GridLayoutGroup>();
        // if (oldGrid != null)
        // {
        //     DestroyImmediate(oldGrid);
        // }

        var oldHorizontal = parent.GetComponent<HorizontalLayoutGroup>();
        if (oldHorizontal != null)
        {
            DestroyImmediate(oldHorizontal);
        }

        // var oldVertical = parent.GetComponent<VerticalLayoutGroup>();
        // if (oldVertical != null)
        // {
        //     DestroyImmediate(oldVertical);
        // }

        // Thêm GridLayoutGroup (tốt nhất cho layout hàng ngang + wrap)
        var grid = parent.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = cellSize;
        grid.spacing = spacing;
        grid.childAlignment = TextAnchor.UpperLeft;
        
        // Constraint: Fixed column count = maxSlotsPerRow
        // Khi đầy sẽ tự động xuống hàng
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = maxSlotsPerRow;

        // Thêm ContentSizeFitter để tự động điều chỉnh kích thước
        var fitter = parent.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = parent.gameObject.AddComponent<ContentSizeFitter>();
        }
        
        // Preferred size cho cả chiều ngang và dọc
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Debug.Log($"<color=green>[InventoryUI]</color> Setup layout for '{rowName}' " +
                  $"(max {maxSlotsPerRow} slots per row)");
    }

    #endregion
}