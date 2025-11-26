using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public static SpawnEnemy Instance;
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null) Instance = this;
    }   

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            SpawnEnemies();
        }
    }
    private void SpawnEnemies()
    {

        int index = Random.Range(0, spawnPoints.Length);
        Vector3 spawnPos = spawnPoints[index].position;

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
    }
    // public Vector3 GetRandomSpawnPoint()
    // {
    //     int index = Random.Range(0, spawnPoints.Length);
    //     return spawnPoints[index].position;
    // }
}
