using System.Collections.Generic;
using UnityEngine;

public class LootDropManager : MonoBehaviour
{
    public static LootDropManager Instance { get; private set; }

    [System.Serializable]
    public struct LootDrop
    {
        public GameObject prefab;   // The item prefab to spawn
        [Range(0f, 1f)] public float dropChance; // Probability of dropping
    }

    public List<LootDrop> lootTable;

    private void Start()
    {
        EnemyManager.Instance.OnDead += EnemyManager_OnDead;

        foreach (var loot in lootTable)
        {
            PoolManager.Instance.CreatePool(loot.prefab, 20, 200);
            Debug.Log("Loot drop table created");
        }
    }

    private void EnemyManager_OnDead(object sender, EnemyManager.OnEnemyDeathEventArgs e)
    {
        Vector3 deathPos = e.DeathPosition;

        foreach (var entry in lootTable)
        {
            if (Random.value < entry.dropChance)
            {
                // Spawn loot via PoolManager instead of Instantiate
                PoolManager.Instance.Spawn(entry.prefab, deathPos, Quaternion.identity);
                Debug.Log($"Loot drop at: {deathPos}");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
