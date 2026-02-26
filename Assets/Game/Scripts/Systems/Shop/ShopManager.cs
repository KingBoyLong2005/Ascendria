using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ShopDB shopDB;

    [Header("Scroll List")]
    [SerializeField] private Transform  slotContainer;  // Content bên trong ScrollRect
    [SerializeField] private GameObject slotPrefab;     // Prefab có ShopSlotUI

    [Header("Detail Panel")]
    [SerializeField] private ShopUI detailPanel;

    [Header("Top Bar (tuỳ chọn)")]
    [SerializeField] private TMP_Text coinText;

    private readonly List<ShopSlotUI> slots = new List<ShopSlotUI>();
    private ShopSlotUI selectedSlot;

    // ── Lifecycle ──────────────────────────────────────────────────

    private void Start()
    {
        ShopSystem.Instance.Initialize();
        BuildSlotList();
        detailPanel.ShowEmpty();
        RefreshCoin();
    }

    // ── Build list ─────────────────────────────────────────────────

    private void BuildSlotList()
    {
        foreach (Transform t in slotContainer) Destroy(t.gameObject);
        slots.Clear();

        foreach (var def in shopDB.items)
        {
            var go   = Instantiate(slotPrefab, slotContainer);
            var slot = go.GetComponent<ShopSlotUI>();

            if (slot == null)
            {
                Debug.LogError($"Prefab '{slotPrefab.name}' thiếu component ShopSlotUI!");
                continue;
            }

            slot.Init(def, OnSlotClicked);
            slots.Add(slot);
        }
    }

    // ── Slot clicked ───────────────────────────────────────────────

    private void OnSlotClicked(string itemId)
    {
        selectedSlot?.SetHighlight(false);

        selectedSlot = slots.Find(s => s.itemId == itemId);
        selectedSlot?.SetHighlight(true);

        detailPanel.ShowItem(itemId, OnUpgradeRequested);
    }

    // ── Upgrade ────────────────────────────────────────────────────

    private void OnUpgradeRequested(string itemId)
    {
        bool ok = ShopSystem.Instance.TryLevelUp(itemId);

        if (ok)
        {
            slots.Find(s => s.itemId == itemId)?.RefreshLevel();
            detailPanel.Refresh();
            RefreshCoin();
            Debug.Log($"[Shop] '{itemId}' nâng lên Lv {ShopSystem.Instance.GetCurrentLevel(itemId)}");
        }
        else
        {
            Debug.Log("[Shop] Nâng cấp thất bại — không đủ coin hoặc đã MAX.");
            // TODO: hiện popup "Không đủ coin"
        }
    }

    // ── Coin ───────────────────────────────────────────────────────

    private void RefreshCoin()
    {
        if (coinText == null) return;
        // TODO: thay bằng GameSaveSystem.Instance.GetCoin() khi sẵn sàng
        // coinText.text = $"Coin: {GameSaveSystem.Instance.GetCoin()}";
        coinText.text = "Coin: ---";
    }
}