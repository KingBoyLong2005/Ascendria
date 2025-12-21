using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static SpawnPointManager;



public class MapManager01 : MonoBehaviour 
{
    private GameObject mapPrefab;
    private GameObject bossGatePrefab;
    
    private Transform mapSpawnPoint;
    
    private GameObject currentMapInstance;
    private GameObject currentBossGateInstance;
    
    private SpawnPointManager currentSpawnPointManager;
    public event EventHandler OnMapReady;

    public NavMeshManager navMeshManager;

    void Awake()
    {
        mapPrefab = PrefabDatabase.Instance.firstMapPrefab;
        bossGatePrefab = PrefabDatabase.Instance.bossGatePrefab;

        navMeshManager = gameObject.AddComponent<NavMeshManager>();
    }

    private void Start()
    {
        LoadMapAndInitializeManagers();
        OnMapReady?.Invoke(this, EventArgs.Empty);
    }
    private void LoadMapAndInitializeManagers()
    {
        Debug.Log("<color=yellow>[MapManager]</color> Bắt đầu tải Map và khởi tạo các Manager...");

        // 1. Logic tải Map (giữ nguyên)
        if (currentMapInstance != null)
        {
            GameObject.Destroy(currentMapInstance);
        }

        if (mapPrefab != null)
        {
            currentMapInstance = GameObject.Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);

            // 2. Tìm kiếm SpawnPointManager ngay sau khi Map được tạo
            currentSpawnPointManager = currentMapInstance.GetComponentInChildren<SpawnPointManager>();

            // 3. Tìm Map Spawn Point mặc định (Vị trí dự phòng nếu cần)
            Transform defaultSpawnPoint = currentMapInstance.transform.Find("DefaultSpawnPoint");
            if (defaultSpawnPoint != null)
            {
                mapSpawnPoint = defaultSpawnPoint;
            }

            NavMeshManager.Instance.LoadNavMesh();
        }
        else
        {
            Debug.LogError("[MapManager] Thiếu Map Prefab.");
        }
        // 4. Tạo Boss Gate sau khi Map và SpawnPointManager đã sẵn sàng
        SpawnBossGate();
        // 5. Phát nhạc nền của Map
        AudioManager.Instance.PlayMusic(PrefabDatabase.Instance.mapTheme);
    }

    // Hàm Spawn Boss Gate 
    private void SpawnBossGate()
    {
        if (bossGatePrefab == null)
        {
            Debug.LogError("[MapManager] Thiếu Boss Gate Prefab. Không thể tạo Boss Gate.");
            return;
        }

        // Lấy Transform (vị trí) cố định của Boss Gate
        Transform spawnTransform = GetFixedPosition(SpawnType.BossGate);

        if (spawnTransform != null)
        {
            // Xóa instance cũ nếu có
            if (currentBossGateInstance != null)
            {
                Destroy(currentBossGateInstance);
            }

            // Instantiate Boss Gate tại vị trí và rotation của điểm spawn cố định
            currentBossGateInstance = GameObject.Instantiate(
                bossGatePrefab,
                spawnTransform.position,
                spawnTransform.rotation,
                spawnTransform // Đặt Boss Gate làm con của điểm spawn để giữ tổ chức trong Hierarchy
            );
            Debug.Log($"<color=green>[MapManager]</color> Boss Gate đã được tạo thành công tại {spawnTransform.position}.");

            // GẮN VÀ KHỞI TẠO BossGateTrigger
            BossGateTrigger gateTrigger = currentBossGateInstance.AddComponent<BossGateTrigger>();
            if (gateTrigger != null)
            {
                // Truyền Transform của điểm spawn (spawnTransform) cho Trigger
                gateTrigger.Initialize(spawnTransform);
            }

            Debug.Log($"<color=green>[MapManager]</color> Boss Gate đã được tạo thành công.");
        }
        else
        {
            Debug.LogWarning("[MapManager] Không tìm thấy điểm spawn Boss Gate cố định (SpawnType.BossGate). Không thể tạo Boss Gate.");
        }
    }

    //-------------------------------------------------------------
    // HÀM LẤY VỊ TRÍ CHO PLAYER (Ngẫu nhiên)
    //-------------------------------------------------------------
    /// <summary>
    /// Trả về một vị trí spawn ngẫu nhiên dành cho Player.
    /// </summary>
    public Vector3 GetPlayerRandomPos()
    {
        // Sử dụng hàm GetSpawnPointsByType để lấy danh sách điểm Player
        List<Transform> availableSpawnPoints = GetSpawnPointsByType(SpawnType.Player);

        if (availableSpawnPoints != null && availableSpawnPoints.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
            Transform selectedPoint = availableSpawnPoints[randomIndex];
            Debug.Log($"Chọn Spawn Point #{randomIndex + 1} / {availableSpawnPoints.Count} ngẫu nhiên (Player).");
            return selectedPoint.position; // TRẢ VỀ VỊ TRÍ (Vector3)
        }

        // Trường hợp không tìm thấy điểm spawn Player
        Debug.LogWarning("[MapManager] Không tìm thấy điểm spawn Player ngẫu nhiên. Trả về vị trí mặc định.");

        if (mapSpawnPoint != null)
        {
            return mapSpawnPoint.position + new Vector3(0, 5, 0); // Vị trí an toàn mặc định
        }

        return Vector3.zero;
    }

    //-------------------------------------------------------------
    // HÀM LẤY VỊ TRÍ CỐ ĐỊNH (Ví dụ: Boss Gate)
    //-------------------------------------------------------------
    /// <summary>
    /// Trả về Transform của vị trí cố định theo loại (ví dụ: BossGate).
    /// </summary>
    public Transform GetFixedPosition(SpawnType type)
    {
        List<Transform> fixedPoints = GetSpawnPointsByType(type);

        if (fixedPoints.Count > 0)
        {
            // Trả về vị trí đầu tiên trong danh sách (giả định vị trí cố định thường là duy nhất)
            return fixedPoints[0];
        }

        Debug.LogWarning($"[MapManager] Không tìm thấy vị trí cố định cho loại {type}.");
        return null;
    }

    //-------------------------------------------------------------
    // HÀM HỖ TRỢ CHUNG
    //-------------------------------------------------------------
    /// <summary>
    /// Hàm private để lấy danh sách điểm spawn đã được lọc theo loại từ SpawnPointManager.
    /// </summary>
    private List<Transform> GetSpawnPointsByType(SpawnType type)
    {
        if (currentSpawnPointManager != null)
        {
            return currentSpawnPointManager.GetSpawnPointsByType(type);
        }

        Debug.LogError("[MapManager] SpawnPointManager chưa được khởi tạo.");
        return new List<Transform>();
    }
}