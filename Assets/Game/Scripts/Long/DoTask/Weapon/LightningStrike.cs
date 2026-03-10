using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    private float damage;
    private float aoeRadius;
    private LayerMask enemyMask;
    private float effectDuration = 1f;
    private float timer = 0f;
    private bool hasStruck = false;

    public void Initialize(float dmg, float radius, LayerMask mask)
    {
        damage = dmg;
        aoeRadius = radius;
        enemyMask = mask;
        hasStruck = false;
        timer = 0f;

        Strike();
    }

    private void Strike()
    {
        if (hasStruck) return;
        hasStruck = true;

        // Raycast straight down from above to find ground impact point
        Ray ray = new Ray(transform.position, Vector3.down);
        Vector3 impactPoint = transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            impactPoint = hit.point;
        }
        else
        {
            // If nothing hit, use lower position
            impactPoint = transform.position + Vector3.down * 10f;
        }

        // Deal AOE damage at impact point
        HitBoxManager.Instance.RequestSphere(
            impactPoint,
            aoeRadius,
            enemyMask,
            OnHitEnemy
        );
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= effectDuration)
        {
            ReturnToPool();
        }
    }

    private void OnHitEnemy(Collider col)
    {
        var enemy = col.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
            WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, damage);
            Debug.Log($"<color=cyan>⚡ Lightning hit {enemy.name} for {damage}</color>");
        }
    }

    private void ReturnToPool()
    {
        PoolManager.Despawn(gameObject, PoolManager.PoolType.GameObject);
    }

    // Visualize AOE radius in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            Gizmos.DrawWireSphere(hit.point, aoeRadius);
        }
        else
        {
            Vector3 impactPoint = transform.position + Vector3.down * 10f;
            Gizmos.DrawWireSphere(impactPoint, aoeRadius);
        }
    }
}