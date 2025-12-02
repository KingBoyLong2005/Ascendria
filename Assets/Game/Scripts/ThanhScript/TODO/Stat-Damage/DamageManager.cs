// DamageManager.cs — listens for hits, calculates damage, applies to player
using UnityEngine;
using System;

public class DamageManager : MonoBehaviour
{
    private void OnEnable()
    {
        EnemyManager.Instance.OnEnemyHitPlayer += EnemyManager_OnEnemyHitPlayer;
    }

    private void EnemyManager_OnEnemyHitPlayer(object sender, EnemyManager.OnHitEventArgs e)
    {
        // e.baseDamage — you can add modifiers here: armor, resistances, criticals...
        float finalDamage = CalculateDamage(e.baseDamage, e.enemy);

        // then apply damage to player
        PlayerStatManager.Instance.ApplyDamage(finalDamage);
    }

    private void OnDisable()
    {
        EnemyManager.Instance.OnEnemyHitPlayer -= EnemyManager_OnEnemyHitPlayer;
    }

    private float CalculateDamage(float baseDamage, GameObject enemy)
    {
        // example: no modifiers yet — just return base
        return baseDamage;
    }
}

