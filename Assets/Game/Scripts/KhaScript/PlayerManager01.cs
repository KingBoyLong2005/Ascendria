using System.Collections.Generic;
using UnityEngine;

public class PlayerManager01 : MonoBehaviour
{
    private GameObject playerPrefab;

    public void Initialize()
    {
        playerPrefab = PrefabDatabase.Instance.playerPrefab;
        //playerSpawnPos = GameManager.Instance.GetMapManager.GetPlayerRandomPos();
        //if (playerSpawnPos != null)
        //{
        //    SpawnPlayer(playerSpawnPos);
        //}
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

