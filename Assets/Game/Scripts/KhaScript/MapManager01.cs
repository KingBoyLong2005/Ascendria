using UnityEngine;
using System.Collections.Generic;


public class MapManager01
{
    private GameObject mapPrefab;
    private Transform mapSpawnPoint;
    private GameObject currentMapInstance;

    public List<Vector3> zxc;

    // Constructor: được gọi bởi GameManager để khởi tạo và truyền dữ liệu cần thiết
    public MapManager01(GameObject prefab, Transform spawnPoint)
    {
        this.mapPrefab = prefab;
        this.mapSpawnPoint = spawnPoint;

        // ĐĂNG KÝ: Lắng nghe sự kiện ngay khi đối tượng được tạo
        GameManager.Instance.OnGameStart += LoadMap;
        Debug.Log("MapManager (Class): Đăng ký lắng nghe GameStart qua GM Instance.");
    }

    // Phương thức dọn dẹp (phải được gọi thủ công bởi GameManager)
    public void Dispose()
    {
        // HỦY ĐĂNG KÝ MỚI
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart -= LoadMap;
        }
        Debug.Log("MapManager (Class): Hủy đăng ký.");
    }

    private SpawnPointManager currentSpawnPointManager; // Thêm biến này

    public void LoadMap()
    {
        Debug.Log("<color=yellow>[MapManager]</color> Bắt đầu tải Map...");

        // Xử lý hủy map cũ
        if (currentMapInstance != null)
        {
            // Phải dùng GameObject.Destroy() vì đây không phải MonoBehaviour
            GameObject.Destroy(currentMapInstance);
        }

        // Khởi tạo Map mới
        if (mapPrefab != null && mapSpawnPoint != null)
        {
            Quaternion x = Quaternion.Euler(-90f, 0f, 0f);
            // Phải dùng GameObject.Instantiate()
            currentMapInstance = GameObject.Instantiate(mapPrefab, mapSpawnPoint.position, x);
            // Lấy SpawnPointManager từ Map vừa tạo
            currentSpawnPointManager = currentMapInstance.GetComponent<SpawnPointManager>();

            if (currentSpawnPointManager == null)
            {
                Debug.LogError("Map Prefab thiếu component SpawnPointManager!");
                return;
            }

            Debug.Log("Map Prefab đã được tạo thành công.");
        }
        else
        {
            Debug.LogError("[MapManager] Thiếu Map Prefab hoặc Spawn Point.");
            return;
        }

        // Kích hoạt sự kiện hoàn thành
        FinishLoading();
    }

    private void FinishLoading()
    {
        Debug.Log("<color=yellow>[MapManager]</color> Map đã tải xong.");

        // TRUYỀN DỮ LIỆU SPAWN POINT khi kích hoạt sự kiện
        if (currentSpawnPointManager != null)
        {
            // Kích hoạt sự kiện Map Loaded và truyền danh sách điểm spawn
            GameManager.Instance.TriggerMapLoaded(currentSpawnPointManager.playerSpawnPoints);
        }
        else
        {
            // Nếu không có spawn point, vẫn báo hiệu tải xong nhưng PlayerManager sẽ phải xử lý lỗi
            GameManager.Instance.TriggerMapLoaded(null);
        }
    }
}
