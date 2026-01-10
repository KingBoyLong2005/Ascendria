using System;
using System.Collections.Generic;
using UnityEngine;
using static EnemyManager;
using static UnityEngine.EventSystems.EventTrigger;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;
    
    //private GameObject bossPrefab;
    private List<GameObject> bossPrefabs = new List<GameObject>();

    private Transform player;

    // Biến để lưu trữ đối tượng Boss đang hoạt động (nếu có)
    public GameObject currentGateBossInstance;

    // Event có thể dùng để báo hiệu Boss đã Spawn xong, hoặc Boss đã chết
    public event EventHandler<OnBossSpawnedEventArgs> OnBossSpawned;
    public class OnBossSpawnedEventArgs : EventArgs
    {
        public BossStats bossStat;  

        public OnBossSpawnedEventArgs(BossStats bs)
        {
            bossStat = bs;
        }
    }

    public event EventHandler<OnBossDieEventArgs> OnBossDie;
    public class OnBossDieEventArgs : EventArgs
    {
        public BossStats bossStat;  // which enemy did hit

        public OnBossDieEventArgs(BossStats bs)
        {
            bossStat = bs;
        }
    }

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GameplayEvents.OnBossGateInteracted += GameplayEvents_OnBossGateInteracted;

        LoadPrefabsFromDatabase();
    }

    private void GameplayEvents_OnBossGateInteracted(Interactable obj)
    {
        Transform pos = obj.transform;
        SpawnGateBoss(pos);
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

    public void SpawnGateBoss(Transform spawnPoint)
    {
        // 2. Kiểm tra vị trí spawn
        if (spawnPoint != null)
        {
            //if(currentGateBossInstance != null)
            //{
            //    Debug.Log("Gate Boss still Alive");
            //    return;
            //}
                
            currentGateBossInstance = SpawnBossInstance(spawnPoint.position, spawnPoint.rotation);

            var bossStat = currentGateBossInstance.GetComponent<BossStats>();
            
            // 5. Kích hoạt Event
            if (currentGateBossInstance != null)
            {
                OnBossSpawned?.Invoke(this, new OnBossSpawnedEventArgs(bossStat));
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

    public void BossDie(BossStats boss)
    {
        OnBossDie?.Invoke(this, new OnBossDieEventArgs(boss));
    }
}