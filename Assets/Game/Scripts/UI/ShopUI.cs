using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ───────────────────────────────────────────────────────────────────
//  SHOP DETAIL UI
// ───────────────────────────────────────────────────────────────────
public class ShopUI : MonoBehaviour
{
    [Header("Empty State")]
    // [SerializeField] private GameObject emptyHint;

    [Header("Info Group")]
    // [SerializeField] private GameObject infoGroup;
    [SerializeField] private TMP_Text   nameText;
    [SerializeField] private TMP_Text   descText;
    [SerializeField] private TMP_Text   levelText;
    [SerializeField] private TMP_Text   valueText;
    [SerializeField] private TMP_Text   upgradeInfoText;

    [Header("Button")]
    [SerializeField] private Button   upgradeButton;
    [SerializeField] private TMP_Text upgradeBtnText;

    private string currentId;
    private System.Action<string> onUpgrade;

    // ── Lifecycle ──────────────────────────────────────────────────

    private void Awake()
    {
        upgradeButton.onClick.AddListener(() => onUpgrade?.Invoke(currentId));
        ShowEmpty();
    }

    // ── Public API ─────────────────────────────────────────────────

    public void ShowItem(string itemId, System.Action<string> upgradeCallback)
    {
        currentId = itemId;
        onUpgrade = upgradeCallback;

        // emptyHint.SetActive(false);
        // infoGroup.SetActive(true);

        Refresh();
    }

    public void Refresh()
    {
        if (string.IsNullOrEmpty(currentId)) return;

        var  def    = ShopSystem.Instance.GetDefinition(currentId);
        if (def == null) return;

        int   level = ShopSystem.Instance.GetCurrentLevel(currentId);
        float value = ShopSystem.Instance.GetCurrentValue(currentId);
        bool  isMax = ShopSystem.Instance.IsMaxLevel(currentId);
        int   cost  = ShopSystem.Instance.GetUpgradeCost(currentId);

        nameText.text  = def.displayName;
        descText.text  = def.description;

        levelText.text = level <= 0
            ? "Chưa mua"
            : $"Level {level} / {def.maxLevel}";

        valueText.text = level > 0
            ? $"Hiệu lực hiện tại: {value}"
            : "Chưa có hiệu lực";

        if (isMax)
        {
            upgradeInfoText.text       = "Đã đạt cấp tối đa";
            upgradeBtnText.text        = "MAX";
            upgradeButton.interactable = false;
        }
        else
        {
            int   nextLv  = level + 1;
            var   nextData = def.GetLevelData(nextLv);
            float nextVal  = nextData != null ? nextData.value : 0f;

            upgradeInfoText.text = level <= 0
                ? $"Mua lần đầu → Lv 1  |  Hiệu lực: {nextVal}  |  Giá: {cost} coin"
                : $"Lv {level} → Lv {nextLv}  |  Hiệu lực: {nextVal}  |  Giá: {cost} coin";

            upgradeBtnText.text        = level <= 0 ? "Mua" : "Nâng cấp";
            upgradeButton.interactable = true;
        }
    }

    public void ShowEmpty()
    {
        currentId = null;
        // emptyHint.SetActive(true);
        // infoGroup.SetActive(false);
        upgradeButton.interactable = false;
    }
}
