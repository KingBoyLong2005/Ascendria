using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private HealthBarUI healthBarUI;
    private XPBarUI xpBarUI;
    private TMP_Text killCount;
    private TMP_Text coinCount;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        // Initialize UI elements
        healthBarUI = FindFirstObjectByType<HealthBarUI>();
        xpBarUI = FindFirstObjectByType<XPBarUI>();

        GameObject go = GameObject.FindWithTag("Kill Counter");
        killCount = go.GetComponentInChildren<TMP_Text>();

        go = GameObject.FindWithTag("Coin Counter");
        coinCount = go.GetComponentInChildren<TMP_Text>();
    }
    private void OnEnable()
    {
        PlayerStatManager.Instance.OnPlayerHealthChange += PlayerStatManager_OnPlayerHealthChange;
        LevelManager.Instance.OnXPChanged += LevelManager_OnXPChanged;
        EnemyManager.Instance.OnDead += EnemyManager_OnDead;
    }
    private void OnDisable()
    {
        PlayerStatManager.Instance.OnPlayerHealthChange -= PlayerStatManager_OnPlayerHealthChange;
        LevelManager.Instance.OnXPChanged -= LevelManager_OnXPChanged;
        EnemyManager.Instance.OnDead -= EnemyManager_OnDead;
    }

    private void EnemyManager_OnDead(object sender, EnemyManager.OnEnemyDeathEventArgs e)
    {
        killCount.text = $"{EnemyManager.Instance.GetKillCount()}";
        coinCount.text = $"{InventoryManager.Instance.GetTotalCoins()}";
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
