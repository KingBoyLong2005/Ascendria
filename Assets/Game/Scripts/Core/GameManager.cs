using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Start,
        Running,
        Paused,
        GameOver
    }
    public GameState currentState { get; private set; }
    public event Action<GameState> OnGameStateChanged;

    private MapManager01 mapManager;
    public MapManager01 GetMapManager => mapManager;

    private PlayerManager01 playerManager;
    private InteractableSpawner interactableSpawner;

    private PoolManager poolManager;
    private EnemyManager enemyManager;
    private DamageManager damageManager;

    private LootDropManager lootDropManager;

    private BossManager bossManager;
    private GameEventManager gameEventManager;

    private UIManager uiManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            currentState = GameState.Running;
        }
    }

    private void Start()
    {
        //Tạo MapManager
        mapManager = gameObject.AddComponent<MapManager01>();
        mapManager.OnMapReady += HandleMapReady;


       
 
    }

    private void HandleMapReady(object sender, EventArgs e)
    {
        //Boss
        bossManager = gameObject.AddComponent<BossManager>();

        //GameEventManager (tạo Object riêng để chứa EventManager)
        //Trong GameEventManager khởi tạo các EventHandler
        var eventManagerObject = new GameObject("GameEventManager");
        var gameEventManager = eventManagerObject.AddComponent<GameEventManager>();

        //gameEventManager = gameObject.AddComponent<GameEventManager>();
        //gameEventManager.Initialize(bossManager);

        //Player
        playerManager = gameObject.AddComponent<PlayerManager01>();
        playerManager.Initialize();

        //Interactable Object (có khi chuyển vào map vì nó thuộc về map)
        interactableSpawner = new InteractableSpawner();
        interactableSpawner.SpawnAll();

        //Quản lý pool
        poolManager = gameObject.AddComponent<PoolManager>();
        enemyManager = gameObject.AddComponent<EnemyManager>();
        damageManager = gameObject.AddComponent<DamageManager>();

        lootDropManager = gameObject.AddComponent<LootDropManager>();
        
        uiManager = gameObject.AddComponent<UIManager>();

        //bossManager = gameObject.AddComponent<BossManager>();
        //gameEventManager = gameObject.AddComponent<GameEventManager>();
        //gameEventManager.Initialize(bossManager);
    }

    private void SetState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(newState); //Gọi các event đăng ký tương ứng
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        SetState(GameState.Running);
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("gọi paused game");
            if (currentState == GameState.Running)
                PauseGame();
            else if (currentState == GameState.Paused)
                ResumeGame();
        }
    }


    private void OnDestroy()
    {
    }
}