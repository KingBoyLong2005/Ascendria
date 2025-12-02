// Loot.cs — attach to loot prefab
using UnityEngine;

public class Loot : MonoBehaviour
{
    [Range(0f, 1f)] public float dropChance;

    public float attractSpeed = 10f;

    private Transform playerPos;

    public void Magnetize(Transform playerTransform)
    {
        playerPos = playerTransform;
    }

    public void SpawnLoot(Vector3 deathPos)
    {
        PoolManager.Spawn(this.gameObject, deathPos, Quaternion.identity);
    }

    void Update()
    {
        if (playerPos != null)
        {
            // Move toward player
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerPos.position,
                attractSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // or your player tag
        {
            // Collect it 
            Collect();
        }
    }

    void Collect()
    {
        // e.g. add to player coins / exp

        playerPos = null; //reset magnet effect when returned to pool

        // then return to pool / deactivate
        PoolManager.Despawn(this.gameObject, PoolManager.PoolType.GameObject);
        Debug.Log("Loot Collected");
    }
}
