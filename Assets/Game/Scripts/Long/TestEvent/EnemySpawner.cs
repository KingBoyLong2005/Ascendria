using UnityEngine;
using Unity.Netcode;

public class EnemySpawner : NetworkBehaviour
{
    public static EnemySpawner Instance;

    public GameObject enemyPrefab; // Cube/Sphere prefab, phải có NetworkObject
    public Transform[] spawnPoints;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // spawn wave đầu khi server start
            SpawnNormalWave();
        }
    }

    void Update()
    {
        if (!IsServer) return; // chỉ server mới spawn

        if (Input.GetKeyDown(KeyCode.I))
        {
            SpawnNormalWave();
        }
    }

    public void SpawnNormalWave()
    {
        Debug.Log("[SERVER] Spawn wave thường");
        for (int i = 0; i < 5; i++)
        {
            SpawnEnemy();
        }
    }

    public void SpawnEventEnemies()
    {
        Debug.Log("[SERVER] Spawn quái cho event");
        for (int i = 0; i < 5; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        int index = Random.Range(0, spawnPoints.Length);
        Vector3 spawnPos = spawnPoints[index].position;

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        enemy.GetComponent<NetworkObject>().Spawn(true);
    }

    // Hàm mới: chỉ lấy vị trí random
    public Vector3 GetRandomSpawnPoint()
    {
        int index = Random.Range(0, spawnPoints.Length);
        return spawnPoints[index].position;
    }
}
