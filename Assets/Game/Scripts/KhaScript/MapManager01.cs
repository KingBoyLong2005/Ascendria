using UnityEngine;
using System.Collections.Generic;
using System;


public class MapManager01 : MonoBehaviour 
{
    private GameObject mapPrefab;
    private Transform mapSpawnPoint;
    private GameObject currentMapInstance;
    private SpawnPointManager currentSpawnPointManager;
    public event EventHandler OnMapReady;

    void Awake()
    {
        mapPrefab = PrefabDatabase.Instance.firstMapPrefab;
    }

    private void Start()
    {
        LoadMapAndGetPlayerSpawnPos();
        OnMapReady?.Invoke(this, EventArgs.Empty);
    }
    private void LoadMapAndGetPlayerSpawnPos()
    {
        Debug.Log("<color=yellow>[MapManager]</color> Bắt đầu tải Map và lấy vị trí spawn...");

        // 1. Logic tải Map (giữ nguyên)
        if (currentMapInstance != null)
        {
            GameObject.Destroy(currentMapInstance);
        }

        if (mapPrefab != null)
        {
            currentMapInstance = GameObject.Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.LogError("[MapManager] Thiếu Map Prefab hoặc Spawn Point.");
        }

    }
    public Vector3 GetPlayerRandomPos(List<Transform> availableSpawnPoints) // sửa lại không dùng tham số
    {
        if (availableSpawnPoints != null && availableSpawnPoints.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
            Transform selectedPoint = availableSpawnPoints[randomIndex];
            Debug.Log($"Chọn Spawn Point #{randomIndex + 1} / {availableSpawnPoints.Count} ngẫu nhiên.");
            return selectedPoint.position; // TRẢ VỀ VỊ TRÍ (Vector3)
        }

        Debug.LogWarning("[MapManager] Không tìm thấy điểm spawn ngẫu nhiên trên Map. Trả về vị trí mặc định.");
        return mapSpawnPoint.position + new Vector3(0, 5, 0); // Vị trí an toàn trên Map
    }
}