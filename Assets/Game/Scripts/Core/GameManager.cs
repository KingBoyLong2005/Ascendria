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

    private InventoryUI invenUI;
    private PoolManager poolManager;
    private EnemyManager enemyManager;
    private DamageManager damageManager;

    private LootDropManager lootDropManager;

    private GameEventManager gameEventManager;
    private AudioManager audioManager;

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
        invenUI = FindFirstObjectByType<InventoryUI>();
    }

    private void Start()
    {
        // 1 object riêng để chứa AudioManager và được khởi tạo trước cả mapManager
        var audioManagerObject = new GameObject("AudioManager");
        var audioManager = audioManagerObject.AddComponent<AudioManager>();
        //audioManager = gameObject.AddComponent<AudioManager>();
        //Tạo MapManager
        mapManager = gameObject.AddComponent<MapManager01>();
        mapManager.OnMapReady += HandleMapReady;


       
 
    }

    private void HandleMapReady(object sender, EventArgs e)
    {
        //audioManager = gameObject.AddComponent<AudioManager>();

        //GameEventManager (tạo Object riêng để chứa EventManager)
        var eventManagerObject = new GameObject("GameEventManager");
        var gameEventManager = eventManagerObject.AddComponent<GameEventManager>();

        //Player
        playerManager = gameObject.AddComponent<PlayerManager01>();
        playerManager.Initialize();

        //Quản lý pool
        poolManager = gameObject.AddComponent<PoolManager>();
        enemyManager = gameObject.AddComponent<EnemyManager>();
        damageManager = gameObject.AddComponent<DamageManager>();

        //Interactable Object (có khi chuyển vào map vì nó thuộc về map)
        interactableSpawner = new InteractableSpawner();
        interactableSpawner.SpawnAll();

        lootDropManager = gameObject.AddComponent<LootDropManager>();
        
        uiManager = gameObject.AddComponent<UIManager>();
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
            invenUI.Toggle();
        }
    }


    private void OnDestroy()
    {
    }
}