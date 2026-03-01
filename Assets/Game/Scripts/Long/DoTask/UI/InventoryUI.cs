using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// InventoryUI được tổ chức thành 3 hàng ngang:
/// - Hàng 1 (trên cùng): Weapons - Mỗi weapon chỉ hiện 1 lần
/// - Hàng 2 (giữa): Buffs - Mỗi buff chỉ hiện 1 lần  
/// - Hàng 3 (dưới): Items - Hiển thị số lượng stackable
/// 
/// + UI Scene: Hiển thị weapons và buffs trong gameplay (dưới healthbar)
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject inventoryPanel;

    [Header("Sections - 3 Rows (Inventory Panel)")]
    [Tooltip("Hàng 1 - Weapons (trên cùng)")]
    public Transform weaponsParent;
    
    [Tooltip("Hàng 2 - Buffs (giữa)")]
    public Transform buffsParent;
    
    [Tooltip("Hàng 3 - Items (dưới)")]
    public Transform itemsParent;
    
    [Header("UI Scene - Gameplay Display")]
    [Tooltip("Hiển thị weapons trong gameplay ")]
    public Transform weaponsParentUIScene;
    
    [Tooltip("Hiển thị buffs trong gameplay ")]
    public Transform buffsParentUIScene;
    
    [Header("Slot Prefab")]
    public GameObject slotPrefab;

    [Header("Layout Settings")]
    [Tooltip("Kích thước mỗi ô slot")]
    public Vector2 cellSize = new Vector2(100, 100);
    
    [Tooltip("Khoảng cách giữa các slot")]
    public Vector2 spacing = new Vector2(10, 10);
    
    [Tooltip("Số slot tối đa trên 1 hàng ngang")]
    public int maxSlotsPerRow = 6;

    [Header("UI Scene Layout Settings")]
    [Tooltip("Kích thước slot cho UI Scene (nhỏ hơn)")]
    public Vector2 cellSizeUIScene = new Vector2(60, 60);
    
    [Tooltip("Khoảng cách giữa các slot UI Scene")]
    public Vector2 spacingUIScene = new Vector2(5, 5);
    
    [Tooltip("Số slot tối đa trên 1 hàng cho UI Scene")]
    public int maxSlotsPerRowUIScene = 8;

    readonly List<GameObject> spawnedSlots = new();
    readonly List<GameObject> spawnedSlotsUIScene = new();
    public Button ResumeButton;
    public Button QuitButton;
    
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

        // Setup layout cho Inventory Panel (3 hàng)
        SetupRowLayout(weaponsParent, "Weapons Row", cellSize, spacing, maxSlotsPerRow);
        SetupRowLayout(buffsParent, "Buffs Row", cellSize, spacing, maxSlotsPerRow);
        SetupRowLayout(itemsParent, "Items Row", cellSize, spacing, maxSlotsPerRow);
        
        // Setup layout cho UI Scene (2 hàng - weapons và buffs)
        SetupRowLayout(weaponsParentUIScene, "Weapons Row UI Scene", cellSizeUIScene, spacingUIScene, maxSlotsPerRowUIScene);
        SetupRowLayout(buffsParentUIScene, "Buffs Row UI Scene", cellSizeUIScene, spacingUIScene, maxSlotsPerRowUIScene);
        
        // Cập nhật UI Scene lần đầu
        RefreshUIScene();
        ResumeButton.onClick.AddListener( () => Close());
        QuitButton.onClick.AddListener(()=> SceneManager.LoadScene("MenuScene"));
    }

    #region Public API

    /// <summary>
    /// Mở inventory kèm pause game + bật chuột (dùng khi state Running → Paused).
    /// </summary>
    public void Open()
    {
        FindFirstObjectByType<TPCameraController>().TurnOnMouse();
        GameManager.Instance.PauseGame();
        OpenOnly();
    }

    /// <summary>
    /// Đóng inventory kèm resume game + tắt chuột (dùng khi state Paused → Running).
    /// </summary>
    public void Close()
    {
        FindFirstObjectByType<TPCameraController>().TurnOffMouse();
        GameManager.Instance.ResumeGame();
        CloseOnly();
    }

    /// <summary>
    /// Chỉ hiện panel inventory — KHÔNG đụng GameState hay chuột.
    /// Dùng khi đang LevelUp mà Esc mở inventory.
    /// </summary>
    public void OpenOnly()
    {
        if (isOpen) return;
        isOpen = true;
        FindFirstObjectByType<TPCameraController>().TurnOnMouse();
        inventoryPanel.SetActive(true);
        RefreshAll();
        ResumeButton.gameObject.SetActive(true);
        QuitButton.gameObject.SetActive(true);
        Debug.Log("<color=cyan>[InventoryUI]</color> Inventory opened (only)");
    }

    /// <summary>
    /// Chỉ ẩn panel inventory — KHÔNG đụng GameState hay chuột.
    /// Dùng khi đang LevelUp mà Esc đóng inventory.
    /// </summary>
    public void CloseOnly()
    {
        if (!isOpen) return;
        isOpen = false;
        FindFirstObjectByType<TPCameraController>().TurnOffMouse();
        inventoryPanel.SetActive(false);
        ResumeButton.gameObject.SetActive(false);
        QuitButton.gameObject.SetActive(false);
        Debug.Log("<color=cyan>[InventoryUI]</color> Inventory closed (only)");
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    /// <summary>
    /// Refresh toàn bộ Inventory Panel (3 hàng)
    /// </summary>
    public void RefreshAll()
    {
        // Xóa tất cả slots hiện tại trong Inventory Panel
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
        
        // Cập nhật UI Scene cùng lúc
        RefreshUIScene();
        
        Debug.Log($"<color=green>[InventoryUI]</color> Refreshed inventory UI - " +
                  $"Weapons: {InventoryManager.Instance.ownedWeapons.Count}, " +
                  $"Buffs: {InventoryManager.Instance.ownedBookBuffs.Count}, " +
                  $"Items: {InventoryManager.Instance.ownedItems.Count}");
    }

    /// <summary>
    /// Refresh chỉ UI Scene (weapons và buffs hiển thị trong gameplay)
    /// Gọi method này khi có thay đổi weapon/buff trong gameplay
    /// </summary>
    public void RefreshUIScene()
    {
        // Xóa tất cả slots hiện tại trong UI Scene
        foreach (var go in spawnedSlotsUIScene)
            Destroy(go);
        spawnedSlotsUIScene.Clear();

        if (InventoryManager.Instance == null || slotPrefab == null)
        {
            Debug.LogWarning("<color=orange>[InventoryUI]</color> InventoryManager or slotPrefab is null!");
            return;
        }

        // Spawn UI Scene
        SpawnWeaponsUIScene();
        SpawnBuffsUIScene();
        
        Debug.Log($"<color=green>[InventoryUI]</color> Refreshed UI Scene - " +
                  $"Weapons: {InventoryManager.Instance.ownedWeapons.Count}, " +
                  $"Buffs: {InventoryManager.Instance.ownedBookBuffs.Count}");
    }

    #endregion

    #region Spawn Methods - Inventory Panel

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

    #region Spawn Methods - UI Scene (Gameplay)

    /// <summary>
    /// Spawn Weapons UI Scene (hiển thị trong gameplay)
    /// </summary>
    void SpawnWeaponsUIScene()
    {
        if (weaponsParentUIScene == null)
        {
            Debug.LogWarning("<color=orange>[InventoryUI]</color> weaponsParentUIScene is null!");
            return;
        }

        int count = 0;
        foreach (var w in InventoryManager.Instance.ownedWeapons)
        {
            if (w == null) continue;
            
            var go = Instantiate(slotPrefab, weaponsParentUIScene);
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
                
            spawnedSlotsUIScene.Add(go);
        }
        
        Debug.Log($"<color=cyan>[InventoryUI]</color> UI Scene - Spawned {count} weapons");
    }

    /// <summary>
    /// Spawn Buffs UI Scene (hiển thị trong gameplay)
    /// </summary>
    void SpawnBuffsUIScene()
    {
        if (buffsParentUIScene == null)
        {
            Debug.LogWarning("<color=orange>[InventoryUI]</color> buffsParentUIScene is null!");
            return;
        }

        int count = 0;
        foreach (var bb in InventoryManager.Instance.ownedBookBuffs)
        {
            if (bb == null) continue;
            
            var go = Instantiate(slotPrefab, buffsParentUIScene);
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
                
            spawnedSlotsUIScene.Add(go);
        }
        
        Debug.Log($"<color=cyan>[InventoryUI]</color> UI Scene - Spawned {count} buffs");
    }

    #endregion

    #region Layout Setup

    /// <summary>
    /// Thiết lập GridLayoutGroup cho mỗi hàng
    /// - Sắp xếp theo chiều ngang (left to right)
    /// - Tự động xuống hàng khi đầy maxSlotsPerRow
    /// - Kích thước tự động điều chỉnh theo nội dung
    /// </summary>
    void SetupRowLayout(Transform parent, string rowName, Vector2 size, Vector2 gap, int maxSlots)
    {
        if (parent == null) return;

        var oldHorizontal = parent.GetComponent<HorizontalLayoutGroup>();
        if (oldHorizontal != null)
        {
            DestroyImmediate(oldHorizontal);
        }

        // Thêm GridLayoutGroup (tốt nhất cho layout hàng ngang + wrap)
        var grid = parent.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = size;
        grid.spacing = gap;
        grid.childAlignment = TextAnchor.UpperLeft;
        
        // Constraint: Fixed column count = maxSlots
        // Khi đầy sẽ tự động xuống hàng
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = maxSlots;

        // Thêm ContentSizeFitter để tự động điều chỉnh kích thước
        // var fitter = parent.GetComponent<ContentSizeFitter>();
        // if (fitter == null)
        // {
        //     fitter = parent.gameObject.AddComponent<ContentSizeFitter>();
        // }
        
        // // Preferred size cho cả chiều ngang và dọc
        // fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        // fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Debug.Log($"<color=green>[InventoryUI]</color> Setup layout for '{rowName}' " +
                  $"(max {maxSlots} slots per row)");
    }

    #endregion
}