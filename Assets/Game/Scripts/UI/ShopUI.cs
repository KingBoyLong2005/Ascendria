using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Stat Group")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text upgradeInfoText;
    [SerializeField] private Button   upgradeButton;
    [SerializeField] private TMP_Text upgradeBtnText;

    [Header("Unlock Group (Weapon / Character)")]
    [SerializeField] private TMP_Text nameUnlockText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button   buyButton;
    [SerializeField] private TMP_Text buyBtnText;

    private enum PanelMode { None, Stat, Unlock }
    private PanelMode            currentMode;
    private string               currentStatId;
    private UnlockItemDefinition currentUnlockDef;
    private bool                 currentIsWeapon;

    private System.Action<string>                     onUpgrade;
    private System.Action<UnlockItemDefinition, bool> onBuy;

    private void Awake()
    {
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
        buyButton.onClick.AddListener(OnBuyClicked);
        ShowEmpty();
    }

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
        levelText.text = level <= 0 ? "Not purchased" : $"Level {level} / {def.maxLevel}";
        valueText.text = level > 0  ? $"Current bonus: +{value}" : "No bonus yet";

        if (isMax)
        {
            upgradeInfoText.text       = "Maximum level reached";
            upgradeBtnText.text        = "MAX";
            upgradeButton.interactable = false;
        }
        else
        {
            int   nextLv  = level + 1;
            var   nextData = def.GetLevelData(nextLv);
            float nextVal  = nextData != null ? nextData.value : 0f;

            // ← +x → +y format here
            upgradeInfoText.text = level <= 0
                ? $"First purchase → Lv 1  |  Bonus: +{nextVal}  |  Cost: {cost} coins"
                : $"Lv {level} → Lv {nextLv}  |  +{value} → +{nextVal}  |  Cost: {cost} coins";

            upgradeBtnText.text        = level <= 0 ? "Buy" : "Upgrade";
            upgradeButton.interactable = true;
        }
    }

    private void RefreshUnlock()
    {
        if (currentUnlockDef == null) return;

        nameUnlockText.text = currentUnlockDef.DisplayName;

        bool unlocked = currentIsWeapon
            ? GameSaveSystem.Instance.IsItemUnlocked(currentUnlockDef.id)
            : GameSaveSystem.Instance.IsCharacterUnlocked(currentUnlockDef.id);

        if (unlocked)
        {
            statusText.text = "Already owned";
            costText.gameObject.SetActive(false);
            buyButton.gameObject.SetActive(false);
        }
        else
        {
            statusText.text        = "Locked";
            costText.gameObject.SetActive(true);
            costText.text          = $"Price: {currentUnlockDef.unlockCost} coins";
            buyButton.gameObject.SetActive(true);
            buyBtnText.text        = "Unlock";
            buyButton.interactable = true;
        }
    }

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