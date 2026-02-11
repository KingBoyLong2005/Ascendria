using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float speed;
    [SerializeField] private int maxBounces;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask bounceableMask;
    [SerializeField] private GameObject hitEffectPrefab;

    private Rigidbody rb;
    private Vector3 currentDirection;
    private float lifeTimer = 5f;
    private float lifetimeCounter = 0f;
    private int currentBounces = 0;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("BulletProjectile needs Rigidbody!");
        }
    }

    public void Initialize(float dmg, float spd, int bounces, LayerMask enemyLayer, LayerMask bounceLayer)
    {
        damage = dmg;
        speed = spd;
        maxBounces = bounces;
        enemyMask = enemyLayer;
        bounceableMask = bounceLayer;
        
        currentBounces = 0;
        hasHit = false;
        lifetimeCounter = 0f;
        currentDirection = transform.forward;

        if (rb != null)
        {
            rb.linearVelocity = currentDirection * speed;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    private void Update()
    {
        lifetimeCounter += Time.deltaTime;
        if (lifetimeCounter >= lifeTimer)
        {
            ReturnToPool();
        }

        // Maintain constant speed
        if (rb != null && rb.linearVelocity.magnitude > 0)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if it's an enemy
        if (((1 << collision.gameObject.layer) & enemyMask) != 0)
        {
            HitEnemy(collision.collider);
            ReturnToPool();
            return;
        }

        // Check if it's bounceable
        if (((1 << collision.gameObject.layer) & bounceableMask) != 0)
        {
            if (currentBounces < maxBounces)
            {
                Bounce(collision);
            }
            else
            {
                ReturnToPool();
            }
        }
    }

    private void Bounce(Collision collision)
    {
        currentBounces++;

        // Get the reflection direction
        ContactPoint contact = collision.contacts[0];
        Vector3 reflectDir = Vector3.Reflect(currentDirection, contact.normal);
        currentDirection = reflectDir.normalized;

        // Apply new velocity
        if (rb != null)
        {
            rb.linearVelocity = currentDirection * speed;
        }

        // Rotate to face new direction
        transform.forward = currentDirection;

        // Spawn bounce effect
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
            Destroy(effect, 1f);
        }
    }

    private void HitEnemy(Collider enemyCollider)
    {
        if (hasHit) return;
        hasHit = true;

        var enemy = enemyCollider.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
            WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, damage);
        }

        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
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

    // Visualize bounces in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}