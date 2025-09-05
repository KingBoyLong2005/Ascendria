using System.Threading;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    public GameObject enemyPrefab; // tạm dùng Cube/Sphere
    public Transform[] spawnPoints;

    private float TimeSpawn = 0f;
    private float TimeReset = 100f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        SpawnNormalWave();
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.I))
        // {
        //     SpawnNormalWave();
        // }

        TimeSpawn += Time.deltaTime * 1;
        if (TimeSpawn < 0)
        {
            TimeSpawn = TimeReset;
            SpawnNormalWave();
        }
    }
    public void SpawnNormalWave()
    {
        Debug.Log("Spawn wave thường");
        for (int i = 0; i < 5; i++)
        {
            SpawnEnemy();
        }
    }

    public void SpawnEventEnemies()
    {
        Debug.Log("Spawn quái cho event");
        for (int i = 0; i < 5; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        int index = Random.Range(0, spawnPoints.Length);
        Instantiate(enemyPrefab, spawnPoints[index].position, Quaternion.identity);
    }
    
    // Hàm mới: chỉ lấy vị trí random
    public Vector3 GetRandomSpawnPoint() {
        int index = Random.Range(0, spawnPoints.Length);
        return spawnPoints[index].position;
    }
}
