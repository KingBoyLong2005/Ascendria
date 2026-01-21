using System.Collections.Generic;
using UnityEngine;

public class AuraEffect : MonoBehaviour
{
    private float damage;
    private float auraRadius;
    private float duration;
    private LayerMask enemyMask;
    private Transform owner;

    private float tickRate = 0.5f;      // Damage mỗi 0.5 giây
    private float tickTimer = 0f;
    private float durationTimer = 0f;

    // Track enemies đã bị hit để tránh spam damage
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
    }

    private void Update()
    {
        // Countdown duration
        durationTimer -= Time.deltaTime;
        if (durationTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Tick damage
        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            tickTimer = tickRate;
            DamageEnemiesInRange();
        }

        // Follow player nếu không phải child
        if (owner != null && transform.parent == null)
        {
            transform.position = owner.position;
        }
    }

    private void DamageEnemiesInRange()
    {
        // Tìm tất cả enemy trong AOE
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

    // Visualize aura radius trong Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 1f, 0.3f); // Magenta transparent
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }

    private void OnDestroy()
    {
        enemiesInRange.Clear();
    }
}