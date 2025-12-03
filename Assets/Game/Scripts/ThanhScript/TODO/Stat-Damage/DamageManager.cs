// DamageManager.cs — listens for hits, calculates damage, applies to player
using UnityEngine;
using System;

public class DamageManager : MonoBehaviour
{
    public static DamageManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Start()
    {
        EnemyManager.Instance.OnEnemyHitPlayer += EnemyManager_OnEnemyHitPlayer;
        //WeaponManager.Instance.OnHitEnemy += WeaponManager_OnHitEnemy;
    }

    private void EnemyManager_OnEnemyHitPlayer(object sender, EnemyManager.OnEnemyHitPlayerEventArgs e)
    {
        // e.baseDamage — you can add modifiers here: armor, resistances, criticals...
        float finalDamage = CalculateDamage(e.baseDamage, e.enemy);

        // then apply damage to player
        PlayerStatManager.Instance.ApplyDamage(finalDamage);
    }

    //private void WeaponManager_OnHitEnemy(object sender, WeaponManager.OnWeaponHitEnemyEventArgs e)
    //{
    //    // e.baseDamage — you can add modifiers here: armor, resistances, criticals...
    //    float finalDamage = CalculateWeaponDamage(e.baseDamage, e.enemy);

    //    // then apply damage to enemy
    //    EnemyStats enemyHit = e.enemy.GetComponent<EnemyStats>();
    //    enemyHit.TakeDamage(finalDamage);
    //}

    private void OnDisable()
    {
        EnemyManager.Instance.OnEnemyHitPlayer -= EnemyManager_OnEnemyHitPlayer;
    }

    private float CalculateDamage(float baseDamage, GameObject enemy)
    {
        // example: no modifiers yet — just return base
        float charArmor = PlayerStatManager.Instance.Armor;
        float finalDamage = baseDamage - charArmor;
        Debug.Log($"Final enemy damage: {finalDamage}");
        return finalDamage;
    }

    private float CalculateWeaponDamage(float baseDamage, GameObject enemy)
    {
        //float final = playerAttack + weaponAttack 
        return baseDamage;
    }
}

