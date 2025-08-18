using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    public static EnemySpawner Instance;

    public GameObject enemyPrefab; // tạm dùng Cube/Sphere
    public Transform[] spawnPoints;

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
    if (Input.GetKeyDown(KeyCode.Space)) {
        SpawnNormalWave();
    }
    }
    public void SpawnNormalWave()
    {
        Debug.Log("Spawn wave thường");
        for (int i = 0; i < 3; i++)
        {
            SpawnEnemy();
        }
    }

    public void SpawnEventEnemies() {
        Debug.Log("Spawn quái cho event");
        for (int i = 0; i < 5; i++) {
            SpawnEnemy();
        }
    }

    void SpawnEnemy() {
        int index = Random.Range(0, spawnPoints.Length);
        Instantiate(enemyPrefab, spawnPoints[index].position, Quaternion.identity);
    }
}
