using System;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    private GameObject bossPrefab;

    // Biến để lưu trữ đối tượng Boss đang hoạt động (nếu có)
    private GameObject currentBossInstance;

    // Event có thể dùng để báo hiệu Boss đã Spawn xong, hoặc Boss đã chết
    public event EventHandler OnBossSpawned;
    // public event EventHandler OnBossDefeated; 

    /// <summary>
    /// Hàm khởi tạo, tải Prefab của Boss.
    /// (Sẽ được gọi bởi GameManager sau khi tạo Manager này).
    /// </summary>
    public void Initialize()
    {
        // 1. Tải Boss Prefab
        // Giả định PrefabDatabase có đường dẫn đến Boss Prefab chính
        bossPrefab = PrefabDatabase.Instance.bossPrefab;

        if (bossPrefab == null)
        {
            Debug.LogError("[BossManager] Thiếu Boss Prefab trong PrefabDatabase.");
        }

        // Không gọi event OnBossSpawned ở đây vì Boss chưa được spawn.
    }

    /// <summary>
    /// Sinh ra Boss tại vị trí được chỉ định (từ Boss Gate).
    /// </summary>
    /// <param name="spawnPoint">Transform của vị trí Boss Gate.</param>
    public void SpawnBoss(Transform spawnPoint)
    {
        if (bossPrefab == null)
        {
            // Thử Initialize lại nếu chưa được gọi hoặc thất bại
            Debug.LogWarning("<color=yellow>[BossManager] Thử khởi tạo lại Boss Prefab trong SpawnBoss.");
            Initialize();
            
            if (bossPrefab == null) return;
        }

        // 2. Kiểm tra vị trí spawn
        if (spawnPoint != null)
        {
            // 3. Xóa Boss cũ nếu có (tránh spawn đè)
            if (currentBossInstance != null)
            {
                Destroy(currentBossInstance);
            }

            // 4. Sinh ra Boss
            Debug.Log("<color=red>[BossManager]</color> Đã gọi hàm SpawnBoss.");
            currentBossInstance = SpawnBossInstance(spawnPoint.position, spawnPoint.rotation);
            

            // 5. Kích hoạt Event
            if (currentBossInstance != null)
            {
                Debug.unityLogger.logHandler.LogFormat(LogType.Log, null, "<color=red>[BossManager]</color> Sự kiện OnBossSpawned đã được kích hoạt.");
                OnBossSpawned?.Invoke(this, EventArgs.Empty);
                
            }
        }
        else
        {
            Debug.LogError("[BossManager] Vị trí Spawn Boss không hợp lệ.");
        }
    }

    private GameObject SpawnBossInstance(Vector3 spawnPos, Quaternion spawnRot)
    {
        if (bossPrefab != null)
        {
            GameObject bossInstance = GameObject.Instantiate(bossPrefab, spawnPos, spawnRot);
            Debug.Log("<color=red>[BossManager]</color> Boss đã được Spawn thành công!");
            return bossInstance;
        }
        else
        {
            Debug.LogError("[BossManager] Thiếu Boss Prefab.");
            return null;
        }
    }
}