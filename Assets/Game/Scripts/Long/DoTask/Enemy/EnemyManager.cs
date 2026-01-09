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
    
    public float minSpawnDistance = 10f;
    public float maxSpawnDistance = 20f;

    public bool ActiveByButton = false;
    public LayerMask groundMask;

    //Spawn Timer
    public float spawnInterval = 3f;
    private float timer = 0f;

    //Difficulty Scale Timer
    public float difficultyMultiplier = 1f;
    private float elapsedTime = 0f;
    private float nextDiff = 10f;

    //Game Countdown Timer
    public float countdown = 20f; // in seconds
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
        if (enemyPrefabs.Count == 0)
            return;
        
        GameObject prefab = null;
        if (!countdownFinished)
        {
            prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)];
        }
        else
        {
            prefab = enemyDemonPrefab;
        }

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
        ++killCount;
        InventoryManager.Instance.AddCoins();
        OnDead?.Invoke(this, new OnEnemyDeathEventArgs{DeathPosition = enemy.transform.position});
    }

    public void EnemyHitPlayer(GameObject enemy, float enemyAttack)
    {
        OnEnemyHitPlayer?.Invoke(this,new OnEnemyHitPlayerEventArgs(enemy,enemyAttack));
    }
}