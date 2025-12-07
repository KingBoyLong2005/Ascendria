using UnityEngine;
using System;
using System.Collections.Generic;

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


        poolManager = gameObject.AddComponent<PoolManager>();
        enemyManager = gameObject.AddComponent<EnemyManager>();
        damageManager = gameObject.AddComponent<DamageManager>();
    }

    private void HandleMapReady(object sender, EventArgs e)
    {
        playerManager = gameObject.AddComponent<PlayerManager01>();
        playerManager.Initialize();

        interactableSpawner = new InteractableSpawner();
        interactableSpawner.SpawnAll();
    }





    private void OnDestroy()
    {
    }
}