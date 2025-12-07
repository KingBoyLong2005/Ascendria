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
        //WeaponManager.Instance.OnWeaponHitEnemy += WeaponManager_OnWeaponHitEnemy;
    }

    private void OnDisable()
    {
        EnemyManager.Instance.OnEnemyHitPlayer -= EnemyManager_OnEnemyHitPlayer;
    }

    private void EnemyManager_OnEnemyHitPlayer(object sender, EnemyManager.OnEnemyHitPlayerEventArgs e)
    {
        float finalDamage = CalculateEnemyDamage(e.enemyAttack);

        // then apply damage to player
        PlayerStatManager.Instance.TakeDamage(finalDamage);
    }

    private void WeaponManager_OnWeaponHitEnemy(object sender, WeaponManager.OnWeaponHitEnemyEventArgs e)
    {
        var enemyHit = e.enemy.GetComponent<EnemyStats>();

        float finalDamage = CalculateWeaponDamage(e.weaponAttack, enemyHit.Armor);

        // then apply damage to enemy
        enemyHit.TakeDamage(finalDamage);
    }

    private float CalculateEnemyDamage(float enemyAttack)
    {
        float charArmor = PlayerStatManager.Instance.Armor;

        float finalDamage = enemyAttack - charArmor;
        Debug.Log($"Final enemy damage: {finalDamage}");

        return finalDamage;
    }

    private float CalculateWeaponDamage(float weaponAttack, float enemyArmor)
    {
        float playerAttack = PlayerStatManager.Instance.Attack;

        float finalDamage = playerAttack + weaponAttack - enemyArmor;

        return finalDamage;
    }
}

