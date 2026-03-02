using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance {get; private set;}

    private BossManager bossManager;

    private Transform player;
    public List<GameObject> enemyPrefabs = new List<GameObject>();      // Danh sách prefab quái
    private GameObject enemyDemonPrefab;
    
    public float minSpawnDistance = 20f;
    public float maxSpawnDistance = 30f;

    public bool ActiveByButton = false;
    public LayerMask groundMask;

    //Spawn Timer
    public float spawnInterval = 3f;
    private float timer = 0f;

    //Difficulty Scale Timer
    public float difficultyMultiplier = 1f;
    private float elapsedTime = 0f;
    private float nextDiff = 60f;

    //Game Countdown Timer
    public float countdown = 600f; // in seconds
    // 600 is normal
    private bool countdownFinished  = false;

    //Kill Count
    private float killCount = 0f;

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

        bossManager = gameObject.AddComponent<BossManager>();

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
        if (PrefabDatabase.Instance.enemyDemonPrefab != null)
        {
            enemyDemonPrefab = PrefabDatabase.Instance.enemyDemonPrefab;
            Debug.Log("Demon added from db to manager");
        }
        // Repeat for all prefab fields — or use reflection/array if many
    }

    private void Update()
    {
        //Spawn Timer
        timer += Time.deltaTime;
        if (timer >= spawnInterval && !ActiveByButton)
        {
            timer = 0f;
            SpawnRandomEnemy();
        }

        if (ActiveByButton && Input.GetKeyDown(KeyCode.P))
        {
            //Transform offsetTransform = GetOffsetTransform(player, new Vector3(5, 5, 5));
            //bossManager.SpawnBoss(offsetTransform);
            SpawnRandomEnemy();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ActiveByButton = !ActiveByButton;
        }

        //Difficult Scale Timer
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= nextDiff)
        { 
            elapsedTime = 0f;
            if (!countdownFinished)
                difficultyMultiplier += 0.5f;
            else
                difficultyMultiplier += 1f;
        }

        //Game Countdown Timer
        if (!countdownFinished)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f)
            {
                countdown = 0f;
                countdownFinished = true;
                //OnCountdownFinished();
            }
        }
    }

    public Transform GetOffsetTransform(Transform player, Vector3 offset)
    {
        // Create a temporary object at the offset
        GameObject temp = new GameObject("OffsetTransform");
        temp.transform.position = player.position + offset;
        temp.transform.rotation = player.rotation; // optional, copy rotation

        return temp.transform;
    }

    public float GetKillCount()
    {
        return killCount;
    }

    // ============================
    //       SPAWN ENEMY
    // ============================
    private void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Count == 0) return;

        GameObject prefab = !countdownFinished 
            ? enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)] 
            : enemyDemonPrefab;

        Vector3 spawnPoint;
        int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Dùng Random.onUnitCircle thay vì insideUnitCircle để tránh vector ~0
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float distance = UnityEngine.Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector3 spawnXZ = player.position + new Vector3(dir.x, 0f, dir.y) * distance;
            Vector3 rayStart = spawnXZ + Vector3.up * 100f;

            if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f, groundMask))
                continue;

            spawnPoint = hit.point;

            // ✅ Kiểm tra khoảng cách thực tế sau khi snap xuống ground
            float actualDist = Vector3.Distance(
                new Vector3(spawnPoint.x, 0, spawnPoint.z),
                new Vector3(player.position.x, 0, player.position.z)
            );
            if (actualDist < minSpawnDistance)
                continue;

            // ✅ Kiểm tra không có vật cản giữa spawn point và player (tùy chọn)
            // Vector3 dirToPlayer = (player.position - spawnPoint).normalized;
            // if (Physics.Raycast(spawnPoint + Vector3.up, dirToPlayer, actualDist, groundMask))
            //     continue;

            var enemyInstance = PoolManager.Spawn(prefab, spawnPoint, Quaternion.identity);
            enemyInstance.GetComponent<EnemyAI>().Setup(player);
            return;
        }

        Debug.LogWarning("EnemyManager: Không tìm được điểm spawn hợp lệ sau " + maxAttempts + " lần thử.");
    }

    // ============================
    //       ENEMY DIE
    // ============================
    public void EnemyDie(GameObject enemy)
    {
        // Trước khi despawn → gửi tín hiệu cho DropManager
        ++killCount;
        InventoryManager.Instance.AddCoins();
        OnDead?.Invoke(this, new OnEnemyDeathEventArgs{DeathPosition = enemy.transform.position});
    }

    public void EnemyHitPlayer(GameObject enemy, float enemyAttack)
    {
        OnEnemyHitPlayer?.Invoke(this,new OnEnemyHitPlayerEventArgs(enemy,enemyAttack));
    }
}