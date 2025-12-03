using System;
using System.Collections.Generic;
using Unity.AI.Navigation.Editor;
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

    public event EventHandler<OnEnemyDeathEventArgs> OnDead;
    public class OnEnemyDeathEventArgs : EventArgs
    {
        public Vector3 DeathPosition;
    }

    public event EventHandler<OnEnemyHitPlayerEventArgs> OnEnemyHitPlayer;
    public class OnEnemyHitPlayerEventArgs : EventArgs
    {
        public GameObject enemy;  // which enemy did hit
        public float baseDamage;

        public OnEnemyHitPlayerEventArgs(GameObject enemy, float damage)
        {
            this.enemy = enemy;
            baseDamage = damage;
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
    }
    private void Start()
    {
        NavMeshManager.Instance.LoadNavMesh();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
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
        Debug.Log($"Tín hiệu event enemy chêt: {enemy.transform.position}");
    }

    public void EnemyHitPlayer(GameObject enemy, float damage)
    {
        OnEnemyHitPlayer?.Invoke(this,new OnEnemyHitPlayerEventArgs(enemy,damage));
    }
}