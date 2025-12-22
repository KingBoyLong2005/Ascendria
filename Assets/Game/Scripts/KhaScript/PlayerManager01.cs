using System;
using UnityEngine;

public class PlayerManager01 : MonoBehaviour
{
    private GameObject playerPrefab;
    private MapManager01 mapManager;

    private InventoryManager inventoryManager;
    private HitBoxManager hitBoxManager;
    private WeaponManager weaponManager;
    private ItemManager itemManager;
    private BookBuffManager bookBuffManager;
    private LevelManager levelManager;
    public event EventHandler OnPlayerReady;
    public void Initialize()
    {
        playerPrefab = PrefabDatabase.Instance.playerPrefab;

        mapManager = FindAnyObjectByType<MapManager01>();
        if (mapManager != null)
        {
            // 3. Lấy vị trí spawn ngẫu nhiên từ MapManager01
            // Hàm này đã được sửa để không còn tham số.
            Vector3 playerSpawnPos = mapManager.GetPlayerRandomPos();

            // 4. Spawn Player
            if (playerSpawnPos != Vector3.zero)
            {
                SpawnPlayer(playerSpawnPos);
                inventoryManager = gameObject.AddComponent<InventoryManager>();
                hitBoxManager = gameObject.AddComponent<HitBoxManager>();
                weaponManager = gameObject.AddComponent<WeaponManager>();  
                itemManager = gameObject.AddComponent<ItemManager>();  
                bookBuffManager = gameObject.AddComponent<BookBuffManager>();
                levelManager = gameObject.AddComponent<LevelManager>();
            }
            else
            {
                Debug.LogError("[PlayerManager] Không thể lấy được vị trí spawn Player hợp lệ.");
            }
        }
        else
        {
            Debug.LogError("[PlayerManager] Không tìm thấy MapManager01. Không thể spawn Player.");
        }
        OnPlayerReady?.Invoke(this, EventArgs.Empty);
    }
    private GameObject SpawnPlayer(Vector3 spawnPos)
    {
        if (playerPrefab != null)
        {
            GameObject playerInstance = GameObject.Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            Debug.Log("<color=blue>[PlayerManager]</color> Player đã được Spawn thành công!");
            // Gọi AudioManager phát tiếng spawn lấy từ Database
            //AudioManager.Instance.PlaySFX(PrefabDatabase.Instance.playerSpawnSfx);
            return playerInstance;
        }
        else
        {
            Debug.LogError("[PlayerManager] Thiếu Player Prefab.");
            return null;
        }
    }
}

