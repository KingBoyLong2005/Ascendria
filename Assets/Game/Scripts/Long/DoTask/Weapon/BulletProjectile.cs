using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float speed;
    [SerializeField] private int maxBounces;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask bounceableMask;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float sphereRadius = 0.2f;

    private Vector3 currentDirection;
    private float lifeTimer = 5f;
    private float lifetimeCounter = 0f;
    private int currentBounces = 0;
    private bool hasHit = false;
    private bool isReturned = false;

    public void Initialize(float dmg, float spd, int bounces, LayerMask enemyLayer, LayerMask bounceLayer)
    {
        damage = dmg;
        speed = spd;
        maxBounces = bounces;
        enemyMask = enemyLayer;
        bounceableMask = bounceLayer;

        currentDirection = transform.forward.normalized;

        currentBounces = 0;
        hasHit = false;
        lifetimeCounter = 0f;
        isReturned = false;
    }

    private void Update()
    {
        if (isReturned) return;

        lifetimeCounter += Time.deltaTime;
        if (lifetimeCounter >= lifeTimer)
        {
            ReturnToPool();
            return;
        }

        MoveBullet();
    }

    private void MoveBullet()
    {
        float moveDistance = speed * Time.deltaTime;

        RaycastHit hit;
        int combinedMask = enemyMask | bounceableMask;

        if (Physics.SphereCast(transform.position, sphereRadius, currentDirection, out hit, moveDistance, combinedMask))
        {
            HandleHit(hit);
        }
        else
        {
            transform.position += currentDirection * moveDistance;
        }
    }

    private void HandleHit(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;
        bool isEnemy = ((1 << hitObject.layer) & enemyMask) != 0;
        bool isBounceable = ((1 << hitObject.layer) & bounceableMask) != 0;

        if (isEnemy)
        {
            HitEnemy(hit.collider);
        }

        if (isEnemy || isBounceable)
        {
            if (currentBounces < maxBounces)
            {
                Bounce(hit);
            }
            else
            {
                ReturnToPool();
            }
        }
        else
        {
            ReturnToPool();
        }
    }

    private void Bounce(RaycastHit hit)
    {
        currentBounces++;

        Vector3 reflectDir = Vector3.Reflect(currentDirection, hit.normal);
        currentDirection = reflectDir.normalized;

        transform.position = hit.point + hit.normal * 0.05f; // tránh kẹt
        transform.forward = currentDirection;

        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
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
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }

    private void ReturnToPool()
    {
        if (isReturned) return;
        isReturned = true;

        PoolManager.Despawn(gameObject, PoolManager.PoolType.GameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}
