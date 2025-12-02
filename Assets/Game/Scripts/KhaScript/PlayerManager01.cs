using System.Collections.Generic;
using UnityEngine;

public class PlayerManager01 : MonoBehaviour
{
    private GameObject playerPrefab;
    private GameManager gameManager;
    private PrefabDatabase prefabDatabase;

    public void Initialize(Vector3 playerSpawnPos)
    {
        gameManager = GameManager.Instance;
        prefabDatabase = gameManager.prefabDatabase;
        playerPrefab = prefabDatabase.playerPrefab;
        if (playerSpawnPos != null)
        {
            SpawnPlayer(playerSpawnPos);
        }
    }
    private GameObject SpawnPlayer(Vector3 spawnPosition)
    {
        if (playerPrefab != null)
        {
            GameObject playerInstance = GameObject.Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("<color=blue>[PlayerManager]</color> Player đã được Spawn thành công!");
            return playerInstance;
        }
        else
        {
            Debug.LogError("[PlayerManager] Thiếu Player Prefab.");
            return null;
        }
    }
}

