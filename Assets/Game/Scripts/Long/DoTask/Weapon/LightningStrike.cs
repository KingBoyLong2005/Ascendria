using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    private float damage;
    private float aoeRadius;
    private LayerMask enemyMask;

    public void Initialize(float dmg, float radius, LayerMask mask)
    {
        damage = dmg;
        aoeRadius = radius;
        enemyMask = mask;

        Strike();
    }

    private void Strike()
    {
        // Raycast thẳng xuống từ trên cao để tìm vị trí chạm đất
        Ray ray = new Ray(transform.position, Vector3.down);
        Vector3 impactPoint = transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            impactPoint = hit.point;
        }
        else
        {
            // Nếu không hit gì, dùng vị trí thấp hơn
            impactPoint = transform.position + Vector3.down * 10f;
        }

        // Gây damage AOE tại điểm chạm
        HitBoxManager.Instance.RequestSphere(
            impactPoint,
            aoeRadius,
            enemyMask,
            OnHitEnemy
        );

        // Tự hủy sau khi effect chạy xong
        Destroy(gameObject, 1f);
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

    // Visualize AOE radius trong Scene view
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