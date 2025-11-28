using System.Collections.Generic;
using UnityEngine;

public class PlayerManager01
{
    private GameObject playerPrefab;
    private Transform spawnPoint;

    // Constructor: được gọi bởi GameManager để khởi tạo và truyền dữ liệu
    public PlayerManager01(GameObject prefab, Transform spawnPoint)
    {
        this.playerPrefab = prefab;
        this.spawnPoint = spawnPoint;

        // // ĐĂNG KÝ: Lắng nghe sự kiện MapLoaded
        // GameManager.Instance.OnMapLoaded += SpawnPlayer;
        // Debug.Log("PlayerManager (Class): Đăng ký lắng nghe MapLoaded qua GM Instance.");
        // ĐĂNG KÝ MỚI: Phương thức SpawnPlayer giờ đây phải nhận List<Transform>
        GameManager.Instance.OnMapLoaded += SpawnPlayer;
        Debug.Log("PlayerManager (Class): Đăng ký lắng nghe MapLoaded qua GM Instance.");
    }

    public void Dispose()
    {
        // HỦY ĐĂNG KÝ MỚI
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMapLoaded -= SpawnPlayer;
        }
        Debug.Log("PlayerManager (Class): Hủy đăng ký.");
    }

    public void SpawnPlayer(List<Transform> availableSpawnPoints)
    {
        Debug.Log("<color=blue>[PlayerManager]</color> Map đã sẵn sàng, bắt đầu Spawn Player...");

        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerManager] Thiếu Player Prefab.");
            return;
        }

        Transform selectedSpawnPoint = null;

        if (availableSpawnPoints != null && availableSpawnPoints.Count > 0)
        {
            // 1. CHỌN NGẪU NHIÊN một điểm spawn
            int randomIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
            selectedSpawnPoint = availableSpawnPoints[randomIndex];
            Debug.Log($"Chọn Spawn Point #{randomIndex + 1} / {availableSpawnPoints.Count} ngẫu nhiên.");
        }
        else
        {
            // 2. Sử dụng điểm mặc định nếu không tìm thấy điểm ngẫu nhiên
            selectedSpawnPoint = spawnPoint;
            Debug.LogWarning("[PlayerManager] Không tìm thấy điểm spawn ngẫu nhiên. Sử dụng điểm mặc định.");
        }

        // 3. Spawn Player
        if (selectedSpawnPoint != null)
        {
            GameObject.Instantiate(playerPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
            Debug.Log("<color=blue>[PlayerManager]</color> Player đã được Spawn thành công!");
        }
    }
}

