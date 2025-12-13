using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Start,
        Running,
        Stop,
        GameOver
    }
    public static GameManager Instance { get; private set; }

    private MapManager01 mapManager;
    public MapManager01 GetMapManager => mapManager;

    private PlayerManager01 playerManager;
    private InteractableSpawner interactableSpawner;

    private PoolManager poolManager;
    private EnemyManager enemyManager;
    private DamageManager damageManager;

    private InventoryManager inventoryManager;
    private HitBoxManager hitBoxManager;
    private WeaponManager weaponManager;
    private BookBuffManager bookBuffManager;
    private LevelManager levelManager;
    private LootDropManager lootDropManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
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
        playerManager = gameObject.AddComponent<PlayerManager01>();
        playerManager.Initialize();

        interactableSpawner = new InteractableSpawner();
        interactableSpawner.SpawnAll();

        poolManager = gameObject.AddComponent<PoolManager>();
        enemyManager = gameObject.AddComponent<EnemyManager>();
        damageManager = gameObject.AddComponent<DamageManager>();

        inventoryManager = gameObject.AddComponent<InventoryManager>();
        hitBoxManager = gameObject.AddComponent<HitBoxManager>();
        weaponManager = gameObject.AddComponent<WeaponManager>();    
        bookBuffManager = gameObject.AddComponent<BookBuffManager>();
        levelManager = gameObject.AddComponent<LevelManager>();
        lootDropManager = gameObject.AddComponent<LootDropManager>();

    }





    private void OnDestroy()
    {
    }
}