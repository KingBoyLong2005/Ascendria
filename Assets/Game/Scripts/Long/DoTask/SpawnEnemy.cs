using UnityEngine;
using UnityEngine.UIElements;

public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public GameObject enemyPrefab;

    public float minSpawnDistance = 10f;
    public float maxSpawnDistance = 20f;

    public bool ActiveByButton = true;
    public LayerMask groundMask;

    public float spawnInterval = 2f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && !ActiveByButton)
        {
            timer = 0f;
            SpawnEnemy();
        }
        else if (ActiveByButton && Input.GetKeyDown(KeyCode.P))
        {
            SpawnEnemy();
        }
    }
    public void SpawnEnemy()
    {
        for (int i = 0; i < 20; i++) // thử tối đa 20 lần
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

            // Random vị trí XZ quanh player
            Vector3 spawnXZ = player.position + new Vector3(dir.x, 0f, dir.y) * distance;

            // Raycast từ trên cao xuống
            Vector3 rayStart = spawnXZ + Vector3.up * 100f;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f, groundMask))
            {
                // hit.point là vị trí mặt đất
                Instantiate(enemyPrefab, hit.point, Quaternion.identity);
                return;
            }
        }

        Debug.Log("Không tìm được vị trí spawn hợp lệ!");
    }
}
