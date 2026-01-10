using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private LevelUpUI levelUpUI;
    private InventoryUI inventoryUI;
    private HealthBarUI healthBarUI;
    private XPBarUI xpBarUI;
    private TMP_Text killCount;
    private TMP_Text coinCount;
    private TMP_Text countdownTimer;

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

        go = GameObject.FindWithTag("Countdown Timer");
        countdownTimer = go.GetComponentInChildren<TMP_Text>();

        levelUpUI = FindFirstObjectByType<LevelUpUI>();
        inventoryUI = FindFirstObjectByType<InventoryUI>();
    }
    private void OnEnable()
    {
        PlayerStatManager.Instance.OnPlayerHealthChange += PlayerStatManager_OnPlayerHealthChange;
        LevelManager.Instance.OnXPChanged += LevelManager_OnXPChanged;
        EnemyManager.Instance.OnDead += EnemyManager_OnDead;

        LevelManager.Instance.OnLevelUp += HandleLevelUp;
        LevelManager.Instance.OnUpgradeApplied += HandleUpgradeApplied;

        InventoryManager.Instance.OnInventoryChanged += HandleInventoryChanged;
        InventoryManager.Instance.OnActiveWeaponsChanged += HandleInventoryChanged;
        InventoryManager.Instance.OnActiveBookBuffsChanged += HandleInventoryChanged;
    }
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.I))
        //     inventoryUI.Toggle();
        UpdateTimerDisplay(EnemyManager.Instance.countdown);
    }
    private void OnDisable()
    {
        PlayerStatManager.Instance.OnPlayerHealthChange -= PlayerStatManager_OnPlayerHealthChange;
        LevelManager.Instance.OnXPChanged -= LevelManager_OnXPChanged;
        EnemyManager.Instance.OnDead -= EnemyManager_OnDead;

        LevelManager.Instance.OnLevelUp -= HandleLevelUp;
        LevelManager.Instance.OnUpgradeApplied -= HandleUpgradeApplied;

        InventoryManager.Instance.OnInventoryChanged -= HandleInventoryChanged;
        InventoryManager.Instance.OnActiveWeaponsChanged -= HandleInventoryChanged;
        InventoryManager.Instance.OnActiveBookBuffsChanged -= HandleInventoryChanged;
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

    void HandleLevelUp(object sender, LevelManager.LevelUpEventArgs e)
    {
        xpBarUI.StartRainbowEffect();
        levelUpUI.Show(e);
    }

    void HandleUpgradeApplied(object sender, LevelManager.UpgradeSelectedEventArgs e)
    {
        xpBarUI.StopRainbowEffect();
        levelUpUI.Hide();
    }

    void HandleInventoryChanged(object sender, System.EventArgs e)
    {
        inventoryUI.RefreshAll();
    }

    void UpdateTimerDisplay(float time)
    {
        // clamp so it doesn't go negative
        time = Mathf.Max(time, 0);

        // get whole minutes and seconds
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        // format mm:ss (two digits each)
        countdownTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}