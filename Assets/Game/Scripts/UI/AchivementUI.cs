using System.Collections.Generic;
using UnityEngine;

/// ═══════════════════════════════════════════════════════════════════
///  ACHIEVEMENT UI SCENE
///  Controller chính build danh sách achievement và xử lý claim.
/// ═══════════════════════════════════════════════════════════════════
public class AchievementUIScene : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private AchievementDatabase achievementDB;

    [Header("Grid")]
    [SerializeField] private Transform  slotContainer;
    [SerializeField] private GameObject slotPrefab;

    private readonly List<AchievementSlotUI> slots = new List<AchievementSlotUI>();

    // ── Lifecycle ──────────────────────────────────────────────────

    private void Start()
    {
        AchievementSystem.Instance.Initialize();
        BuildGrid();
    }

    // ── Build grid ─────────────────────────────────────────────────

    private void BuildGrid()
    {
        foreach (Transform t in slotContainer) Destroy(t.gameObject);
        slots.Clear();

        foreach (var def in achievementDB.achievements)
        {
            var go   = Instantiate(slotPrefab, slotContainer);
            var slot = go.GetComponent<AchievementSlotUI>();

            if (slot == null)
            {
                Debug.LogError($"Prefab '{slotPrefab.name}' thiếu component AchievementSlotUI!");
                continue;
            }

            slot.Init(def, OnClaimRequested);
            slots.Add(slot);
        }
    }

    // ── Claim ──────────────────────────────────────────────────────

    private void OnClaimRequested(string id)
    {
        bool ok = AchievementSystem.Instance.ClaimAchievement(id);

        if (ok)
        {
            // Refresh slot vừa claim để cập nhật UI
            var slot = slots.Find(s => s.ItemId == id);
            slot?.Refresh();

            Debug.Log($"[Achievement] Đã nhận: {id}");

            // TODO: phát coin/reward ở đây nếu cần
            // GameSaveSystem.Instance.AddCoin(rewardAmount);
        }
        else
        {
            Debug.Log($"[Achievement] Claim thất bại: {id} (chưa hoàn thành hoặc đã nhận rồi)");
        }
    }

    // ── Public: refresh toàn bộ (sau ApplyProgressChanges) ──

    public void RefreshAll()
    {
        foreach (var slot in slots)
            slot.Refresh();
    }
}