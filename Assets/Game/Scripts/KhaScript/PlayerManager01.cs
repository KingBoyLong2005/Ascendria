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
    private ProfileCharacterLoader characterLoader;
    public void Initialize()
    {
        playerPrefab = PrefabDatabase.Instance.playerPrefab;

        mapManager = FindAnyObjectByType<MapManager01>();
        if (mapManager != null)
        {
            Vector3 playerSpawnPos = mapManager.GetPlayerRandomPos();

            if (playerSpawnPos != Vector3.zero)
            {
                // Spawn player và lấy instance để apply profile ngay lập tức
                GameObject playerInstance = SpawnPlayer(playerSpawnPos);

                if (playerInstance != null)
                {
                    // Lấy ProfileCharacterLoader từ instance vừa spawn (không dùng FindFirstObject)
                    characterLoader = playerInstance.GetComponent<ProfileCharacterLoader>();
                    if (characterLoader != null)
                    {
                        // Apply character được chọn từ ReadyScene
                        ApplySelectedCharacter();
                    }
                    else
                    {
                        Debug.LogError("[PlayerManager] ProfileCharacterLoader không tìm thấy trên Player Prefab!");
                    }
                }

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
    private void ApplySelectedCharacter()
    {
        ProfileCharacterData selectedCharacter = PrefabDatabase.Instance.selectedCharacter;

        if (selectedCharacter == null)
        {
            // Không có selection từ ReadyScene → dùng profile mặc định đã assign trên prefab
            Debug.LogWarning("[PlayerManager] Không có character được chọn. Dùng profile mặc định trên prefab.");
            if (characterLoader.profile != null)
            {
                characterLoader.ApplyProfile();
            }
            return;
        }

        // Apply character được chọn từ ReadyScene
        characterLoader.profile = selectedCharacter;
        characterLoader.ApplyProfile();

        Debug.Log($"[PlayerManager] Player spawned với character: {selectedCharacter.displayName}");
        Debug.Log($"[PlayerManager] Stats → HP: {characterLoader.MaxHP}, Attack: {characterLoader.attack}, " +
                  $"Armor: {characterLoader.armor}, MoveSpeed: {characterLoader.moveSpeed}");
    }
}