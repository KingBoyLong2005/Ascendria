using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ShopSlotUI : MonoBehaviour
{
    [Header("References")]
    // [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image    highlight;
    [SerializeField] private Button   button;

    [HideInInspector] public string itemId;

    private System.Action<string> onClick;

    // ── Setup ──────────────────────────────────────────────────────

    public void Init(ShopItemDefinition def, System.Action<string> onClickCallback)
    {
        itemId  = def.id;
        onClick = onClickCallback;

        // nameText.text = def.displayName;
        RefreshLevel();
        SetHighlight(false);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(itemId));
    }

    // ── Refresh ────────────────────────────────────────────────────

    public void RefreshLevel()
    {
        int level = ShopSystem.Instance.GetCurrentLevel(itemId);
        var def   = ShopSystem.Instance.GetDefinition(itemId);
        if (def == null) return;

        if (level <= 0)
            levelText.text = "Chưa mua";
        else if (ShopSystem.Instance.IsMaxLevel(itemId))
            levelText.text = $"MAX  (Lv {level})";
        else
            levelText.text = $"Lv {level} / {def.maxLevel}";
    }

    // ── Highlight ──────────────────────────────────────────────────

    public void SetHighlight(bool on)
    {
        if (highlight != null)
            highlight.enabled = on;
    }
}