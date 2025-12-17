using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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
}
