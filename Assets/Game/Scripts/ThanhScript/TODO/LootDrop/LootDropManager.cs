using System.Collections.Generic;
using UnityEngine;

public class LootDropManager : MonoBehaviour
{
    public static LootDropManager Instance { get; private set; }

    /*[System.Serializable]
    public struct LootDrop
    {
        public GameObject prefab;   // The item prefab to spawn
        [Range(0f, 1f)] public float dropChance; // Probability of dropping
    }*/

    public List<GameObject> lootTable = new List<GameObject>();

    private void Start()
    {
        lootTable.Add(PrefabDatabase.Instance.health);
        lootTable.Add(PrefabDatabase.Instance.exp);
        EnemyManager.Instance.OnDead += EnemyManager_OnDead;
    }

    private void EnemyManager_OnDead(object sender, EnemyManager.OnEnemyDeathEventArgs e)
    {
        Debug.Log("Rơi đồ");
        Vector3 deathPos = e.DeathPosition;

        foreach (var entry in lootTable)
        {
            var loot = entry.GetComponent<Loot>();
            if (Random.value < loot.dropChance)
            {
                // Spawn loot via PoolManager instead of Instantiate
                loot.SpawnLoot(deathPos);
                Debug.Log($"Loot drop at: {deathPos}");
            }
        }
    }
}
