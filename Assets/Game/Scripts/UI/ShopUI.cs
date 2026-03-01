using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    // ── Stat ───────────────────────────────────────────────────────
    [Header("Stat Group")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text upgradeInfoText;
    [SerializeField] private Button   upgradeButton;
    [SerializeField] private TMP_Text upgradeBtnText;

    // ── Unlock ─────────────────────────────────────────────────────
    [Header("Unlock Group (Weapon / Character)")]
    [SerializeField] private TMP_Text nameUnlockText;  // ← lấy từ DisplayName
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button   buyButton;
    [SerializeField] private TMP_Text buyBtnText;

    // ── Runtime ────────────────────────────────────────────────────
    private enum PanelMode { None, Stat, Unlock }
    private PanelMode            currentMode;
    private string               currentStatId;
    private UnlockItemDefinition currentUnlockDef;
    private bool                 currentIsWeapon;

    private System.Action<string>                     onUpgrade;
    private System.Action<UnlockItemDefinition, bool> onBuy;

    // ── Lifecycle ──────────────────────────────────────────────────

    private void Awake()
    {
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
        buyButton.onClick.AddListener(OnBuyClicked);
        ShowEmpty();
    }

    // ══════════════════════════════════════════════════════════════
    //  PUBLIC API
    // ══════════════════════════════════════════════════════════════

    public void ShowItem(string itemId, System.Action<string> upgradeCallback)
    {
        currentMode   = PanelMode.Stat;
        currentStatId = itemId;
        onUpgrade     = upgradeCallback;
        RefreshStat();
    }

    public void ShowUnlockItem(UnlockItemDefinition def, bool isWeapon, System.Action<UnlockItemDefinition, bool> buyCallback)
    {
        currentMode      = PanelMode.Unlock;
        currentUnlockDef = def;
        currentIsWeapon  = isWeapon;
        onBuy            = buyCallback;
        RefreshUnlock();
    }

    public void Refresh()
    {
        if      (currentMode == PanelMode.Stat)   RefreshStat();
        else if (currentMode == PanelMode.Unlock) RefreshUnlock();
    }

    public void ShowEmpty()
    {
        currentMode                = PanelMode.None;
        upgradeButton.interactable = false;
        buyButton.interactable     = false;
    }

    // ══════════════════════════════════════════════════════════════
    //  REFRESH — STAT
    // ══════════════════════════════════════════════════════════════

    private void RefreshStat()
    {
        var def = ShopSystem.Instance.GetDefinition(currentStatId);
        if (def == null) return;

        int   level = ShopSystem.Instance.GetCurrentLevel(currentStatId);
        float value = ShopSystem.Instance.GetCurrentValue(currentStatId);
        bool  isMax = ShopSystem.Instance.IsMaxLevel(currentStatId);
        int   cost  = ShopSystem.Instance.GetUpgradeCost(currentStatId);

        nameText.text  = def.displayName;
        descText.text  = def.description;
        levelText.text = level <= 0 ? "Chưa mua" : $"Level {level} / {def.maxLevel}";
        valueText.text = level > 0  ? $"Hiệu lực hiện tại: {value}" : "Chưa có hiệu lực";

        if (isMax)
        {
            upgradeInfoText.text       = "Đã đạt cấp tối đa";
            upgradeBtnText.text        = "MAX";
            upgradeButton.interactable = false;
        }
        else
        {
            int   nextLv   = level + 1;
            var   nextData = def.GetLevelData(nextLv);
            float nextVal  = nextData != null ? nextData.value : 0f;

            upgradeInfoText.text = level <= 0
                ? $"Mua lần đầu → Lv 1  |  Hiệu lực: {nextVal}  |  Giá: {cost} coin"
                : $"Lv {level} → Lv {nextLv}  |  Hiệu lực: {nextVal}  |  Giá: {cost} coin";

            upgradeBtnText.text        = level <= 0 ? "Mua" : "Nâng cấp";
            upgradeButton.interactable = true;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  REFRESH — UNLOCK
    // ══════════════════════════════════════════════════════════════

    private void RefreshUnlock()
    {
        if (currentUnlockDef == null) return;

        // ✅ Lấy tên từ DisplayName (đọc sourceWeapon.weaponName hoặc sourceCharacter.name)
        //    KHÔNG dùng ShopSystem.GetDefinition vì weapon/character không có trong ShopDB
        nameUnlockText.text = currentUnlockDef.DisplayName;

        bool unlocked = currentIsWeapon
            ? GameSaveSystem.Instance.IsItemUnlocked(currentUnlockDef.id)
            : GameSaveSystem.Instance.IsCharacterUnlocked(currentUnlockDef.id);

        if (unlocked)
        {
            statusText.text = "Đã sở hữu";
            costText.gameObject.SetActive(false);
            buyButton.gameObject.SetActive(false);
        }
        else
        {
            statusText.text = "Chưa mở khóa";
            costText.gameObject.SetActive(true);
            costText.text          = $"Giá: {currentUnlockDef.unlockCost} coin";
            buyButton.gameObject.SetActive(true);
            buyBtnText.text        = "Mua";
            buyButton.interactable = true;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  BUTTON CALLBACKS
    // ══════════════════════════════════════════════════════════════

    private void OnUpgradeClicked()
    {
        if (currentMode != PanelMode.Stat) return;
        onUpgrade?.Invoke(currentStatId);
    }

    private void OnBuyClicked()
    {
        if (currentMode != PanelMode.Unlock) return;
        onBuy?.Invoke(currentUnlockDef, currentIsWeapon);
    }
}