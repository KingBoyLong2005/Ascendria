using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float speed;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject explosionEffectPrefab;

    private Rigidbody rb;
    private float lifeTimer = 5f;
    private float lifetimeCounter = 0f;
    private bool hasExploded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("FireballProjectile needs Rigidbody!");
        }
    }

    public void Initialize(float dmg, float radius, float spd, LayerMask mask, GameObject effectPrefab = null)
    {
        damage = dmg;
        explosionRadius = radius;
        speed = spd;
        enemyMask = mask;
        explosionEffectPrefab = effectPrefab;
        hasExploded = false;
        lifetimeCounter = 0f;

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
            rb.useGravity = false;
        }
    }

    private void Update()
    {
        lifetimeCounter += Time.deltaTime;
        if (lifetimeCounter >= lifeTimer && !hasExploded)
        {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's an enemy
        if (((1 << other.gameObject.layer) & enemyMask) != 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // ==== HITBOX AoE ====
        HitBoxManager.Instance.RequestSphere(
            transform.position,
            explosionRadius,
            enemyMask,
            OnHitEnemy
        );

        // ==== EFFECT ====
        if (explosionEffectPrefab != null)
        {
            // Scale effect based on explosionRadius
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            float effectScale = explosionRadius / 2f;
            effect.transform.localScale = Vector3.one * effectScale;
        }

        // Return to pool instead of Destroy
        ReturnToPool();
    }

    private void OnHitEnemy(Collider col)
    {
        var enemy = col.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
            WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, damage);
        }
    }

    private void ReturnToPool()
    {
        // Reset velocity before returning to pool
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        PoolManager.Despawn(gameObject, PoolManager.PoolType.GameObject);
    }

    // Visualize explosion radius in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}