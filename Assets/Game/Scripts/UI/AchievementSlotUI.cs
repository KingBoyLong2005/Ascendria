using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// ═══════════════════════════════════════════════════════════════════
///  ACHIEVEMENT SLOT UI  —  Horizontal Row
///
///  TRẠNG THÁI:
///  ┌──────────────────┬────────────┬─────────────┬─────────────┬─────────────┐
///  │ State            │ DimOverlay │ ProgressBar │ ClaimButton │ ClaimedLabel│
///  ├──────────────────┼────────────┼─────────────┼─────────────┼─────────────┤
///  │ Chưa hoàn thành  │  ON (0.55) │     ON      │    OFF      │    OFF      │
///  │ Hoàn thành       │    OFF     │     OFF     │    ON       │    OFF      │
///  │ Đã nhận          │  ON (0.35) │     OFF     │    OFF      │    ON       │
///  └──────────────────┴────────────┴─────────────┴─────────────┴─────────────┘
/// ═══════════════════════════════════════════════════════════════════
public class AchievementSlotUI : MonoBehaviour
{
    [Header("Icon")]
    [SerializeField] private Image dimOverlay;

    [Header("Info")]
    [SerializeField] private TMP_Text   nameText;
    [SerializeField] private TMP_Text   descText;
    [SerializeField] private TMP_Text   progressText;
    [SerializeField] private Image      barFill;
    [SerializeField] private GameObject progressBarRoot; // kéo vào ProgressBar GameObject (cha chứa bar + text)

    [Header("Claim Area")]
    [SerializeField] private Button     claimButton;
    // [SerializeField] private GameObject claimedLabel;

    private string itemId;
    private System.Action<string> onClaim;

    // ── Setup ──────────────────────────────────────────────────────

    public void Init(AchievementDefinition def, System.Action<string> onClaimCallback)
    {
        itemId  = def.id;
        onClaim = onClaimCallback;

        nameText.text = def.displayName;
        descText.text = def.description;

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() => onClaim?.Invoke(itemId));

        Refresh();
    }

    // ── Refresh ────────────────────────────────────────────────────

    public void Refresh()
    {
        var progress = AchievementSystem.Instance.GetProgress(itemId);
        var def      = AchievementSystem.Instance.GetDefinitionPublic(itemId);
        if (progress == null || def == null) return;

        int   cur    = progress.current;
        int   target = def.target;
        float ratio  = target > 0 ? Mathf.Clamp01((float)cur / target) : 0f;

        if (progress.isClaimed)
        {
            // Đã nhận: dim nhẹ, ẩn bar + nút, hiện label "Đã nhận"
            SetDim(0.35f);
            SetProgressBarVisible(true);
            progressText.text = "Claimed"; 
            claimButton.gameObject.SetActive(false);
            // claimedLabel.SetActive(true);
        }
        else if (progress.isCompleted)
        {
            SetDim(0f);
            SetProgressBarVisible(false);                  // hiện bar đầy
            progressText.text = "Complete";               // hiện chữ Complete
            UpdateBar(1f);                                // bar fill 100%
            claimButton.gameObject.SetActive(true);
        }
        else
        {
            // Đang làm: dim, hiện progress bar
            SetDim(0.55f);
            SetProgressBarVisible(true);
            progressText.text = $"{cur} / {target}";
            UpdateBar(ratio);
            claimButton.gameObject.SetActive(false);
            // claimedLabel.SetActive(false);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────

    private void SetDim(float alpha)
    {
        if (dimOverlay == null) return;
        var c = dimOverlay.color;
        c.a = alpha;
        dimOverlay.color = c;
    }

    private void SetProgressBarVisible(bool visible)
    {
        if (progressBarRoot != null)
            progressBarRoot.SetActive(visible);
    }

    private void UpdateBar(float ratio)
    {
        if (barFill == null) return;
        var anchor = barFill.rectTransform.anchorMax;
        anchor.x = ratio;
        barFill.rectTransform.anchorMax = anchor;
    }

    public string ItemId => itemId;
}