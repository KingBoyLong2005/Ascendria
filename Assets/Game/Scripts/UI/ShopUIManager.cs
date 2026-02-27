using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ShopUIManager : MonoBehaviour
{
    private const int ITEMS_PER_ROW = 7;

    [Header("Data")]
    [SerializeField] private ShopDB         shopDB;
    [SerializeField] private UnlockDatabase unlockDB;

    [Header("ScrollView Content")]
    [SerializeField] private Transform content;

    [Header("Prefabs")]
    [SerializeField] private GameObject sectionHeaderPrefab;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject emptySlotPrefab;

    [Header("Spacing")]
    [SerializeField] private float sectionSpacing = 24f;  // khoảng trống giữa các section

    [Header("Detail Panel")]
    [SerializeField] private ShopUI detailPanel;

    [Header("Top Bar")]
    [SerializeField] private TMP_Text coinText;

    // Runtime
    private readonly List<ShopSlotUI> statSlots   = new List<ShopSlotUI>();
    private readonly List<ShopSlotUI> weaponSlots = new List<ShopSlotUI>();
    private readonly List<ShopSlotUI> charSlots   = new List<ShopSlotUI>();
    private ShopSlotUI selectedSlot;

    // ── Lifecycle ──────────────────────────────────────────────────

    private void Start()
    {
        GameSaveSystem.Instance.Initialize();
        ShopSystem.Instance.Initialize();

        foreach (Transform t in content) Destroy(t.gameObject);

        BuildStatSection();
        SpawnSpacer();
        BuildUnlockSection("VŨ KHÍ",   unlockDB.weapons,    weaponSlots, isWeapon: true);
        SpawnSpacer();
        BuildUnlockSection("NHÂN VẬT", unlockDB.characters, charSlots,   isWeapon: false);

        detailPanel.ShowEmpty();
        RefreshCoin();

        StartCoroutine(RebuildNextFrame());
    }

    private System.Collections.IEnumerator RebuildNextFrame()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
    }

    // ══════════════════════════════════════════════════════════════
    //  SPAWN HELPERS
    // ══════════════════════════════════════════════════════════════

    /// Spawn tiêu đề section
    private void SpawnHeader(string title)
    {
        var go   = Instantiate(sectionHeaderPrefab, content);
        var text = go.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = title;
    }

    /// Spawn khoảng trắng giữa các section
    private void SpawnSpacer()
    {
        var go = new GameObject("Spacer", typeof(RectTransform));
        go.transform.SetParent(content, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, sectionSpacing);

        // Thêm LayoutElement để VerticalLayoutGroup nhận diện height
        var le = go.AddComponent<LayoutElement>();
        le.minHeight      = sectionSpacing;
        le.preferredHeight = sectionSpacing;
    }

    /// Chia items thành hàng ITEMS_PER_ROW
    private void SpawnRows(int totalItems, System.Action<Transform, int> spawnSlot)
    {
        if (totalItems <= 0) return;

        int rows = Mathf.CeilToInt((float)totalItems / ITEMS_PER_ROW);
        for (int r = 0; r < rows; r++)
        {
            var rowGO = Instantiate(rowPrefab, content);
            for (int col = 0; col < ITEMS_PER_ROW; col++)
            {
                int index = r * ITEMS_PER_ROW + col;
                if (index < totalItems)
                    spawnSlot(rowGO.transform, index);
                else
                    Instantiate(emptySlotPrefab, rowGO.transform);
            }
        }
    }

    private ShopSlotUI SpawnSlot(Transform parent)
    {
        var go   = Instantiate(slotPrefab, parent);
        var slot = go.GetComponent<ShopSlotUI>();
        if (slot == null) Debug.LogError("slotPrefab thiếu ShopSlotUI!");
        return slot;
    }

    private void SetSelected(ShopSlotUI newSlot)
    {
        selectedSlot?.SetHighlight(false);
        selectedSlot = newSlot;
        selectedSlot?.SetHighlight(true);
    }

    // ══════════════════════════════════════════════════════════════
    //  STAT SECTION
    // ══════════════════════════════════════════════════════════════

    private void BuildStatSection()
    {
        SpawnHeader("STAT");

        var items = shopDB.items;
        SpawnRows(items.Length, (row, i) =>
        {
            var slot = SpawnSlot(row);
            slot.Init(items[i], OnStatSlotClicked);
            statSlots.Add(slot);
        });
    }

    private void OnStatSlotClicked(string itemId)
    {
        SetSelected(statSlots.Find(s => s.itemId == itemId));
        detailPanel.ShowItem(itemId, OnStatUpgradeRequested);
    }

    private void OnStatUpgradeRequested(string itemId)
    {
        bool ok = ShopSystem.Instance.TryLevelUp(itemId);
        if (ok)
        {
            statSlots.Find(s => s.itemId == itemId)?.RefreshLevel();
            detailPanel.Refresh();
            RefreshCoin();
        }
        else
        {
            Debug.Log("[Shop] Nâng cấp thất bại — không đủ coin hoặc đã MAX.");
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  WEAPON / CHARACTER SECTION
    // ══════════════════════════════════════════════════════════════

    private void BuildUnlockSection(
        string                 header,
        UnlockItemDefinition[] defs,
        List<ShopSlotUI>       slotList,
        bool                   isWeapon)
    {
        SpawnHeader(header);

        if (defs == null || defs.Length == 0) return;

        SpawnRows(defs.Length, (row, i) =>
        {
            var slot = SpawnSlot(row);
            // Bấm slot → mở detail (không mua thẳng)
            slot.Init(defs[i], isWeapon, OnUnlockSlotClicked);
            slotList.Add(slot);
        });
    }

    private void OnUnlockSlotClicked(UnlockItemDefinition def, bool isWeapon)
    {
        var allSlots = isWeapon ? weaponSlots : charSlots;
        SetSelected(allSlots.Find(s => s.itemId == def.id));

        // Mở detail panel với thông tin unlock
        detailPanel.ShowUnlockItem(def, isWeapon, OnBuyFromDetail);
    }

    private void OnBuyFromDetail(UnlockItemDefinition def, bool isWeapon)
    {
        int coin = GameSaveSystem.Instance.GetCoin();
        if (coin < def.unlockCost)
        {
            Debug.Log($"[Shop] Không đủ coin — cần {def.unlockCost}, có {coin}");
            return;
        }

        GameSaveSystem.Instance.SpendCoin(def.unlockCost);

        if (isWeapon) GameSaveSystem.Instance.UnlockItem(def.id);
        else          GameSaveSystem.Instance.UnlockCharacter(def.id);

        // Refresh slot + detail
        foreach (var s in weaponSlots) s.Refresh();
        foreach (var s in charSlots)   s.Refresh();
        detailPanel.Refresh();

        RefreshCoin();
        Debug.Log($"[Shop] Đã mở khóa '{def.DisplayName}'!");
    }

    // ── Coin ───────────────────────────────────────────────────────

    private void RefreshCoin()
    {
        if (coinText == null) return;
        coinText.text = $"Coin: {GameSaveSystem.Instance.GetCoin()}";
    }
}