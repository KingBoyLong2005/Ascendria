using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance {get; private set;}

    private Transform player;
    public List<GameObject> enemyPrefabs = new List<GameObject>();      // Danh sách prefab quái
    
    public float minSpawnDistance = 10f;
    public float maxSpawnDistance = 20f;

    public bool ActiveByButton = false;
    public LayerMask groundMask;

    public float spawnInterval = 2f;
    private float timer = 0f;

    public float difficultyMultiplier = 1f;
    private float elapsedTime = 0f;
    private float nextDiff = 60f;

    public event EventHandler<OnEnemyDeathEventArgs> OnDead;
    public class OnEnemyDeathEventArgs : EventArgs
    {
        public Vector3 DeathPosition;
    }

    public event EventHandler<OnEnemyHitPlayerEventArgs> OnEnemyHitPlayer;
    public class OnEnemyHitPlayerEventArgs : EventArgs
    {
        public GameObject enemy;  // which enemy did hit
        public float enemyAttack;

        public OnEnemyHitPlayerEventArgs(GameObject enemy, float damage)
        {
            this.enemy = enemy;
            enemyAttack = damage;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadPrefabsFromDatabase();
        groundMask = LayerMask.GetMask("Ground");
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
        if(enemyPrefabs != null)
            enemyPrefabs.Clear();

        if (PrefabDatabase.Instance.enemyPrefab != null)
        { 
            enemyPrefabs.Add(PrefabDatabase.Instance.enemyPrefab);
            Debug.Log("Enemy added from db to manager");
        }
        //if (prefabDatabase.enemyPrefab2 != null)
        //    enemyPrefabList.Add(prefabDatabase.enemyPrefab2);
        //// Repeat for all prefab fields — or use reflection/array if many
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
            for(int i = 0; i<1; i++)
            {
                SpawnRandomEnemy();
            }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ActiveByButton = !ActiveByButton;
        }

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= nextDiff)
        { 
            elapsedTime = 0f;
            difficultyMultiplier += 0.5f;
        }
    }

    // ============================
    //       SPAWN ENEMY
    // ============================
    void SpawnRandomEnemy()
    {
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
            var enemyInstance = PoolManager.Spawn(prefab, hit.point, Quaternion.identity);
            // Gán callback để quái có thể báo “tao chết rồi”
            enemyInstance.GetComponent<EnemyAI>().Setup(player);
            // return;
        }
    }

    // ============================
    //       ENEMY DIE
    // ============================
    public void EnemyDie(GameObject enemy)
    {
        // Trước khi despawn → gửi tín hiệu cho DropManager
        OnDead?.Invoke(this, new OnEnemyDeathEventArgs{DeathPosition = enemy.transform.position});
    }

    public void EnemyHitPlayer(GameObject enemy, float enemyAttack)
    {
        OnEnemyHitPlayer?.Invoke(this,new OnEnemyHitPlayerEventArgs(enemy,enemyAttack));
    }
}