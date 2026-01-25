using System.Collections.Generic;
using UnityEngine;

public class AuraEffect : MonoBehaviour
{
    private float damage;
    private float auraRadius;
    private float duration;
    private LayerMask enemyMask;
    private Transform owner;

    private float tickRate = 0.5f;      // Damage every 0.5 seconds
    private float tickTimer = 0f;
    private float durationTimer = 0f;

    // Track enemies that have been hit to avoid spam damage
    private HashSet<Collider> enemiesInRange = new HashSet<Collider>();

    public void Initialize(float dmg, float radius, float dur, LayerMask mask, Transform ownerTransform)
    {
        damage = dmg;
        auraRadius = radius;
        duration = dur;
        enemyMask = mask;
        owner = ownerTransform;

        durationTimer = duration;
        tickTimer = 0f;
        
        if (enemiesInRange == null)
            enemiesInRange = new HashSet<Collider>();
        else
            enemiesInRange.Clear();
    }

    private void Update()
    {
        // Countdown duration
        durationTimer -= Time.deltaTime;
        if (durationTimer <= 0f)
        {
            ReturnToPool();
            return;
        }

        // Tick damage
        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            tickTimer = tickRate;
            DamageEnemiesInRange();
        }

        // Follow player if not a child
        if (owner != null && transform.parent == null)
        {
            transform.position = owner.position;
        }
    }

    private void DamageEnemiesInRange()
    {
        // Find all enemies in AOE
        Collider[] enemies = Physics.OverlapSphere(transform.position, auraRadius, enemyMask);

        foreach (Collider enemyCol in enemies)
        {
            var enemy = enemyCol.GetComponentInParent<EnemyStats>();
            if (enemy != null)
            {
                WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, damage);
            }
        }

        if (enemies.Length > 0)
        {
            Debug.Log($"<color=magenta>✨ Aura hit {enemies.Length} enemies for {damage} damage</color>");
        }
    }

    private void ReturnToPool()
    {
        // Detach from parent before returning to pool
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        // Clear enemy tracking
        enemiesInRange.Clear();
        
        PoolManager.Despawn(gameObject, PoolManager.PoolType.GameObject);
    }

    // Visualize aura radius in Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 1f, 0.3f); // Magenta transparent
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }

    private void OnDestroy()
    {
        if (enemiesInRange != null)
            enemiesInRange.Clear();
    }
}