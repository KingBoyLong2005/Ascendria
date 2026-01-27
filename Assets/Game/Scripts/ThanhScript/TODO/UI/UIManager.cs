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
    private OpenChestUI openChestUI;

    private TMP_Text killCount;
    private TMP_Text coinCount;
    private TMP_Text countdownTimer;

    //Boss HP Bars Container
    private GameObject bossHPBarPrefab; 
    private Transform container;
    private Dictionary<EnemyStats, BossHPBarUI> activeBars = new Dictionary<EnemyStats, BossHPBarUI>();

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

        go = GameObject.FindWithTag("Boss HP Container");
        container = go.GetComponent<Transform>();

        bossHPBarPrefab = PrefabDatabase.Instance.bossHPBar;

        levelUpUI = FindFirstObjectByType<LevelUpUI>();
        inventoryUI = FindFirstObjectByType<InventoryUI>();
        openChestUI = FindFirstObjectByType<OpenChestUI>();
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

        BossManager.Instance.OnBossSpawned += BossManager_OnBossSpawned;
        BossManager.Instance.OnBossDie += BossManager_OnBossDie;

        GameplayEvents.OnLootChestCollected += GameplayEvents_OnLootChestCollected;
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

        BossManager.Instance.OnBossSpawned -= BossManager_OnBossSpawned;
        BossManager.Instance.OnBossDie -= BossManager_OnBossDie;

        GameplayEvents.OnLootChestCollected -= GameplayEvents_OnLootChestCollected;
    }

    #region EVENT SIGNAL FUNCTIONS

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

    private void BossManager_OnBossSpawned(object sender, BossManager.OnBossSpawnedEventArgs e)
    {
        AddBossHPBar(e.bossStat);
    }

    private void BossManager_OnBossDie(object sender, BossManager.OnBossDieEventArgs e)
    {
        RemoveBossHPBar(e.bossStat);
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
        inventoryUI.RefreshUIScene();
    }

    private void GameplayEvents_OnLootChestCollected(Item obj)
    {
        openChestUI.Show(obj);
    }

    #endregion

    #region SUPPORT FUNCTIONS

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

    public void AddBossHPBar(BossStats boss) 
    {
        GameObject barObj = Instantiate(bossHPBarPrefab, container); 
        BossHPBarUI barUI = barObj.GetComponent<BossHPBarUI>(); 
        barUI.Init(boss); 
        
        activeBars[boss] = barUI; // store by reference
        ResizeBars();
    }

    public void RemoveBossHPBar(BossStats boss) 
    {
        if (activeBars.TryGetValue(boss, out BossHPBarUI barUI)) 
        { 
            Destroy(barUI.gameObject); 
            activeBars.Remove(boss); 
            ResizeBars(); 
        }
    }

    private void ResizeBars() 
    {
        int count = activeBars.Count; if (count == 0) return; 
        float widthPercent = 1f / count; int i = 0; 
        foreach (var bar in activeBars.Values) 
        { 
            RectTransform rt = bar.GetComponent<RectTransform>(); 
            rt.anchorMin = new Vector2(i * widthPercent, 0); 
            rt.anchorMax = new Vector2((i + 1) * widthPercent, 1); 
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; 
            i++; 
        }
    }

    #endregion
}
