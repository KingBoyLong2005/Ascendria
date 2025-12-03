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
    private PlayerManager01 playerManager;

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

        //Tạo PlayerManager 
        mapManager.OnMapReady += (sender,e) =>
        {
            playerManager = gameObject.AddComponent<PlayerManager01>();
            //playerManager.Initialize(mapManager.GetPlayerRandomPos()); // ở đây không có tham số để truyền vào - function này không nên truyền tham số
        };
    }





    private void OnDestroy()
    {
    }
}