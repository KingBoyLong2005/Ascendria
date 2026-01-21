using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float speed;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject explosionEffectPrefab;

    private Rigidbody rb;
    private float lifeTimer = 5f;  // Tự hủy sau 5s nếu không trúng
    private bool hasExploded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("FireballProjectile cần Rigidbody!");
        }
    }

    public void Initialize(float dmg, float radius, float spd, LayerMask mask, GameObject effectPrefab = null)
    {
        damage = dmg;
        explosionRadius = radius;
        speed = spd;
        enemyMask = mask;
        explosionEffectPrefab = effectPrefab;

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
            rb.useGravity = false;
        }
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f && !hasExploded)
        {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem có phải enemy không
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
            // Scale effect theo explosionRadius
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            float effectScale = explosionRadius / 2f; // Giả sử base radius = 2
            effect.transform.localScale = Vector3.one * effectScale;
        }

        Destroy(gameObject);
    }

    private void OnHitEnemy(Collider col)
    {
        var enemy = col.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
            WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, damage);
        }
    }

    // Visualize explosion radius trong Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}