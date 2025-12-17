using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private InventoryUI invUI;
    private LevelUpUI levelUI;
    private HealthBarUI healthBarUI;
    private XPBarUI xpBarUI;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        healthBarUI = FindFirstObjectByType<HealthBarUI>();
        invUI = FindFirstObjectByType<InventoryUI>();
        levelUI = FindFirstObjectByType<LevelUpUI>();
        RegisterEvent();
        PlayerStatManager.Instance.OnPlayerHealthChange += PlayerStatManager_OnPlayerHealthChange;
        LevelManager.Instance.OnXPChanged += LevelManager_OnXPChanged;

        healthBarUI = FindFirstObjectByType<HealthBarUI>();
        xpBarUI = FindAnyObjectByType<XPBarUI>();
    }

    private void LevelManager_OnXPChanged(object sender, LevelManager.XPProgressEventArgs e)
    {
        xpBarUI.SetXP(e.currXP, e.xpToNext);
    }

    private void PlayerStatManager_OnPlayerHealthChange(object sender, PlayerStatManager.OnPlayerHealthChangeEventArgs e)
    {
        healthBarUI.SetHealth(e.currentHealth, e.maxHealth);
    }
    private void RegisterEvent()
    {
        PlayerStatManager.Instance.OnPlayerHealthChange += PlayerStatManager_OnPlayerHealthChange;
        InventoryManager.Instance.OnInventoryChanged += (s,e) => invUI.RefreshAll();
        InventoryManager.Instance.OnActiveWeaponsChanged += (s,e) => invUI.RefreshAll();
        InventoryManager.Instance.OnActiveBookBuffsChanged += (s,e) => invUI.RefreshAll(); // Added for buffs
        InventoryManager.Instance.OnActiveItemsChanged += (s, e) => invUI.RefreshAll();

        LevelManager.Instance.OnLevelUp +=(s,e) => levelUI.OnLevelUp(s,e);
        LevelManager.Instance.OnUpgradeApplied += (s,e) => levelUI.OnUpgradeApplied(s,e);
    }
    private void OnLevel(object _, LevelManager.LevelUpEventArgs e)
    {
        
    }
}
