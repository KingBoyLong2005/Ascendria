using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class LootDropManager : MonoBehaviour
{
    public static LootDropManager Instance { get; private set; }

    public List<GameObject> lootTable = new List<GameObject>();
    private GameObject lootChest;

    private void Start()
    {
        lootTable.Add(PrefabDatabase.Instance.health);
        lootTable.Add(PrefabDatabase.Instance.exp);
        lootChest = PrefabDatabase.Instance.lootChest;
    }
    private void OnEnable()
    {
        EnemyManager.Instance.OnDead += EnemyManager_OnDead;
        BossManager.Instance.OnBossDie += BossManager_OnBossDead;
    }
    private void OnDisable()
    {
        EnemyManager.Instance.OnDead -= EnemyManager_OnDead;
        BossManager.Instance.OnBossDie -= BossManager_OnBossDead;
    }

    private void EnemyManager_OnDead(object sender, EnemyManager.OnEnemyDeathEventArgs e)
    {
        //Debug.Log("Rơi đồ");
        Vector3 deathPos = e.DeathPosition;

        foreach (var entry in lootTable)
        {
            var loot = entry.GetComponent<Loot>();
            if (Random.value < loot.dropChance)
            {
                // Spawn loot via PoolManager instead of Instantiate
                loot.SpawnLoot(deathPos);
                //Debug.Log($"Loot drop at: {deathPos}");
            }
        }
    }

    private void BossManager_OnBossDead(object sender, BossManager.OnBossDieEventArgs e)
    {
        //Debug.Log("Rơi đồ");
        Vector3 deathPos = e.deathPos;

        var loot = lootChest.GetComponent<Loot>();
        if (Random.value < loot.dropChance)
        {
            // Spawn loot via PoolManager instead of Instantiate
            loot.SpawnLoot(deathPos);
            //Debug.Log($"Loot drop at: {deathPos}");
        }
    }
}
