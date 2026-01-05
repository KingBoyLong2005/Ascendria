using System;
using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;
    
    //private GameObject bossPrefab;
    private List<GameObject> bossPrefabs = new List<GameObject>();

    private Transform player;

    // Biến để lưu trữ đối tượng Boss đang hoạt động (nếu có)
    private GameObject currentBossInstance;

    // Event có thể dùng để báo hiệu Boss đã Spawn xong, hoặc Boss đã chết
    public event EventHandler OnBossSpawned;
    // public event EventHandler OnBossDefeated; 

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadPrefabsFromDatabase();
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    private void LoadPrefabsFromDatabase()
    {
        // clear maybe
        if (bossPrefabs != null)
            bossPrefabs.Clear();

        if (PrefabDatabase.Instance.bossPrefab != null)
        {
            bossPrefabs.Add(PrefabDatabase.Instance.bossPrefab);
            Debug.Log("Enemy added from db to manager");
        }
        // Repeat for all prefab fields — or use reflection/array if many
    }

    public void SpawnBoss(Transform spawnPoint)
    {
        // 2. Kiểm tra vị trí spawn
        if (spawnPoint != null)
        {
            Debug.Log("<color=red>[BossManager]</color> Đã gọi hàm SpawnBoss.");
            currentBossInstance = SpawnBossInstance(spawnPoint.position, spawnPoint.rotation);
            
            // 5. Kích hoạt Event
            if (currentBossInstance != null)
            {
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
        if (bossPrefabs.Count == 0)
            return null;

        GameObject prefab = bossPrefabs[UnityEngine.Random.Range(0, bossPrefabs.Count)];
        
        if (prefab != null)
        {
            GameObject bossInstance = PoolManager.Spawn(prefab, spawnPos, spawnRot);
            bossInstance.GetComponent<EnemyAI>().Setup(player);

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