using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance {get; private set;}
    private Transform player;
    public List<GameObject> enemyPrefabs;      // Danh sách prefab quái
    public float minSpawnDistance = 10f;
    public float maxSpawnDistance = 20f;

    public bool ActiveByButton = true;
    public LayerMask groundMask;

    public float spawnInterval = 2f;
    private float timer;

    // Event quái chết (DropManager sẽ sub vào)
    public delegate void EnemyDiedHandler(GameObject enemyPrefab, Vector3 pos);
    public event EnemyDiedHandler OnEnemyDied;


    public event EventHandler<OnEnemyDeathEventArgs> OnDead;

    public class OnEnemyDeathEventArgs : EventArgs
    {
        public Vector3 DeathPosition;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        NavMeshManager.Instance.BakeNavMesh();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        
        // Tạo pool cho TỪNG prefab
        foreach (var prefab in enemyPrefabs)
        {
            PoolManager.Instance.CreatePool(prefab, 20, 200);
        }
        
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && !ActiveByButton)
        {
            timer = 0f;
            SpawnRandomEnemy();
        }
        else if (ActiveByButton && Input.GetKeyDown(KeyCode.P))
        {
            for(int i = 0; i<20; i++)
            {
                SpawnRandomEnemy();
            }
        }
    }

    // ============================
    //       SPAWN ENEMY
    // ============================
    void SpawnRandomEnemy()
    {
        // var player = GameObject.FindGameObjectWithTag("Player").transform;
        if (enemyPrefabs.Count == 0)
            return;
        GameObject prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)];
        Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
        float distance = UnityEngine.Random.Range(minSpawnDistance, maxSpawnDistance);

        // Random vị trí XZ quanh player
        Vector3 spawnXZ = player.position + new Vector3(dir.x, 0f, dir.y) * distance;

        // Raycast từ trên cao xuống
        Vector3 rayStart = spawnXZ + Vector3.up * 100f;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f, groundMask))
        {
            // hit.point là vị trí mặt đất
            var enemyInstance = PoolManager.Instance.Spawn(prefab, hit.point, Quaternion.identity);
            // Gán callback để quái có thể báo “tao chết rồi”
            enemyInstance.GetComponent<EnemyAI>().Setup(this, prefab, player);
            // return;
        }
    }

    // ============================
    //       ENEMY DIE
    // ============================
    public void EnemyDie(GameObject enemyPrefab, GameObject enemyInstance)
    {
        // Trước khi despawn → gửi tín hiệu cho DropManager
        // OnEnemyDied?.Invoke(enemyPrefab, enemyInstance.transform.position);
        OnDead?.Invoke(enemyPrefab, new OnEnemyDeathEventArgs{DeathPosition = enemyInstance.transform.position});
        Debug.Log($"Tín hiệu event enemy chêt: {enemyInstance.transform.position}");
        // Trả về pool
        // PoolManager.Instance.Despawn(enemyPrefab, enemyInstance);
        

    }



}

// using UnityEngine;

// public class EnemyManager : MonoBehaviour
// {
//     public float lifeTime = 5f;

//     void Awake()
//     {
//         Debug.Log($"Spawn tại {transform.position}");
//     }
//     void Start()
//     {
//         Die();
//     }
//     private void Die()
//     {
//         Destroy(gameObject, lifeTime);
//         Debug.Log($"Enemy chết! InstanceID = {gameObject.GetInstanceID()}");
//     }
// }
