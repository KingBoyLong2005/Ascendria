using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ShopSlotUI : MonoBehaviour
{
    public enum SlotMode { Stat, Unlock }

    [Header("References")]
    [SerializeField] private Image    iconImage;
    [SerializeField] private Image    dimOverlay;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image    highlight;
    [SerializeField] private Button   button;

    [HideInInspector] public string itemId;

    private SlotMode             mode;
    private bool                 isWeapon;
    private UnlockItemDefinition unlockDef;

    private System.Action<string>                          onStatClick;
    private System.Action<UnlockItemDefinition, bool>      onUnlockClick;

    // ══════════════════════════════════════════════════════════════
    //  INIT — STAT
    // ══════════════════════════════════════════════════════════════

    public void Init(ShopItemDefinition def, System.Action<string> onClickCallback)
    {
        mode        = SlotMode.Stat;
        itemId      = def.id;
        onStatClick = onClickCallback;

        // if (iconImage != null && def.icon != null)
        //     iconImage.sprite = def.icon;

        SetDim(0f);
        SetHighlight(false);
        RefreshLevel();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onStatClick?.Invoke(itemId));
    }

    // ══════════════════════════════════════════════════════════════
    //  INIT — UNLOCK
    // ══════════════════════════════════════════════════════════════

    public void Init(UnlockItemDefinition def, bool weapon, System.Action<UnlockItemDefinition, bool> onClickCallback)
    {
        mode          = SlotMode.Unlock;
        itemId        = def.id;
        unlockDef     = def;
        isWeapon      = weapon;
        onUnlockClick = onClickCallback;

        // Lấy icon từ source SO
        if (iconImage != null)
        {
            var icon = def.Icon;   // IUnlockable property, đọc từ sourceWeapon/sourceCharacter
            if (icon != null) iconImage.sprite = icon;
        }

        SetHighlight(false);
        RefreshUnlock();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onUnlockClick?.Invoke(unlockDef, isWeapon));
    }

    // ══════════════════════════════════════════════════════════════
    //  REFRESH
    // ══════════════════════════════════════════════════════════════

    public void Refresh()
    {
        if (mode == SlotMode.Stat) RefreshLevel();
        else                       RefreshUnlock();
    }

    public void RefreshLevel()
    {
        int level = ShopSystem.Instance.GetCurrentLevel(itemId);
        var def   = ShopSystem.Instance.GetDefinition(itemId);
        if (def == null) return;

        levelText.text = level <= 0                             ? "Chưa mua"
                       : ShopSystem.Instance.IsMaxLevel(itemId) ? $"MAX (Lv {level})"
                       :                                          $"Lv {level} / {def.maxLevel}";
    }

    private void RefreshUnlock()
    {
        if (unlockDef == null) return;

        bool unlocked = isWeapon
            ? GameSaveSystem.Instance.IsItemUnlocked(itemId)
            : GameSaveSystem.Instance.IsCharacterUnlocked(itemId);

        SetDim(unlocked ? 0f : 0.6f);
        levelText.text = unlocked ? "Đã mở" : $"{unlockDef.unlockCost} coin";
    }

    // ══════════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════════

    public void SetHighlight(bool on)
    {
        if (highlight != null) highlight.enabled = on;
    }

    private void SetDim(float alpha)
    {
        if (dimOverlay == null) return;
        var c = dimOverlay.color;
        c.a = alpha;
        dimOverlay.color = c;
    }
}