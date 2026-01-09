using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    private float damage;
    private LayerMask enemyMask;

    public void Initialize(float dmg, LayerMask mask)
    {
        damage = dmg;
        enemyMask = mask;

        Strike();
    }

    private void Strike()
    {
        // Raycast thẳng xuống từ trên cao
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 50f, enemyMask))
        {
            EnemyStats enemy = hit.collider.GetComponentInParent<EnemyStats>();
            if (enemy != null)
            {
                WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, damage);
                Debug.Log($"<color=cyan>⚡ Lightning hit {enemy.name} for {damage}</color>");
            }
        }

        Destroy(gameObject, 0.5f); // tồn tại ngắn cho effect
    }
}
