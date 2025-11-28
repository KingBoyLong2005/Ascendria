using UnityEngine;
using System.Collections.Generic;

// KHÔNG kế thừa từ MonoBehaviour
public class BossManager
{
    private GameObject bossPrefab;
    private Transform spawnPoint;

    // Khởi tạo BossManager
    public BossManager(GameObject prefab, Transform spawnPoint)
    {
        this.bossPrefab = prefab;
        this.spawnPoint = spawnPoint;

        // ĐĂNG KÝ: Lắng nghe sự kiện sau khi Map và Player đã sẵn sàng
        GameManager.Instance.OnMapAndPlayerReady += SpawnBoss;
        Debug.Log("BossManager (Class): Đăng ký lắng nghe MapAndPlayerReady.");
    }

    public void Dispose()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMapAndPlayerReady -= SpawnBoss;
        }
        Debug.Log("BossManager (Class): Hủy đăng ký.");
    }

    // Phương thức Spawn Boss, được gọi khi sự kiện kích hoạt
    public void SpawnBoss()
    {
        Debug.Log("<color=red>[BossManager]</color> Bắt đầu Spawn Boss...");

        if (bossPrefab != null && spawnPoint != null)
        {
            // Spawn Boss tại vị trí đã thiết lập trong GameManager Inspector
            GameObject.Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("<color=red>[BossManager]</color> Boss đã được Spawn thành công!");
        }
        else
        {
            Debug.LogError("[BossManager] Thiếu Boss Prefab hoặc Spawn Point.");
        }
    }
}