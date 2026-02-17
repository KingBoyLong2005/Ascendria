// Loot.cs — attach to loot prefab
using UnityEngine;

public class Loot : MonoBehaviour
{
    [Range(0f, 1f)] public float dropChance;
    public float attractSpeed = 10f;
    public float amount = 10f;

    private Transform playerPos;

    public void Magnetize(Transform playerTransform)
    {
        playerPos = playerTransform;
    }

    public void SpawnLoot(Vector3 deathPos)
    {
        RaycastHit hit; 
        if (Physics.Raycast(deathPos, Vector3.down, out hit, Mathf.Infinity)) 
        {
            PoolManager.Spawn(this.gameObject, hit.point, Quaternion.identity);
        }
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
            if (gameObject.CompareTag("EXP Gem"))
            {
                CollectEXP();
            }
            if (gameObject.CompareTag("HP Drop"))
            {
                CollectHealth();
            }
            if (gameObject.CompareTag("Loot Chest"))
            {
                CollectLootChest();
            }
        }
    }

    void CollectEXP()
    {
        // e.g. add to player coins / exp
        LevelManager.Instance.AddXP(amount);

        playerPos = null; //reset magnet effect when returned to pool

        // then return to pool / deactivate
        PoolManager.Despawn(this.gameObject, PoolManager.PoolType.GameObject);
        //Debug.Log("Loot Collected");
    }

    void CollectHealth()
    {
        // e.g. add to player coins / exp
        PlayerStatManager.Instance.RestoreHealth(amount);

        playerPos = null; //reset magnet effect when returned to pool

        // then return to pool / deactivate
        PoolManager.Despawn(this.gameObject, PoolManager.PoolType.GameObject);
        //Debug.Log("Loot Collected");
    }

    void CollectLootChest()
    {
        //Or ChestEventHandler
        var item = ItemManager.Instance.GetRandomItem();
        GameplayEvents.RaiseLootChestCollected(item);

        playerPos = null; //reset magnet effect when returned to pool

        // then return to pool / deactivate
        PoolManager.Despawn(this.gameObject, PoolManager.PoolType.GameObject);
        Debug.Log("Loot Chest Picked Up");
    }
}
