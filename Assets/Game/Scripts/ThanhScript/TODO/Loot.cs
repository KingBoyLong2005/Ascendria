// Loot.cs — attach to loot prefab
using UnityEngine;

public class Loot : MonoBehaviour
{
    public GameObject lootPrefab;
    
    public float attractSpeed = 10f;
    private Transform player;

    public void Magnetize(Transform playerTransform)
    {
        player = playerTransform;
    }

    void Update()
    {
        if (player != null)
        {
            // Move toward player
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                attractSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // or your player tag
        {
            // Collect it (or return to pool)
            Collect();
        }
    }

    void Collect()
    {
        // e.g. add to player coins / exp

        // then return to pool / deactivate
        PoolManager.Instance.Despawn(lootPrefab, this.gameObject);
        Debug.Log("Loot Collected");
    }
}
