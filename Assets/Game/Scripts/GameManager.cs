using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PrefabDatabase prefabDatabase;

    private MapManager01 mapManager;
    private PlayerManager01 playerManager;
    private BossManager bossManager;

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

        //Tạo MapManager
        mapManager = gameObject.AddComponent<MapManager01>();



        //Tạo PlayerManager 
        //mapManager.OnMapReady += () =>
        //{
        //    playerManager = new PlayerManager(mapManager.GetMapData());
        //    playerManager.Initialize(mapManager.GetPlayerRandomPos);

        //};
        //Tạo EnemyManager (include boss)
        //Tạo ...
    }

    



    private void OnDestroy()
    {
    }
}