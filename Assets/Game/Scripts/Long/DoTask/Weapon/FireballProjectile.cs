using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float speed;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject explosionEffectPrefab;

    private Rigidbody rb;
    private float lifeTimer = 3f;  // Tự hủy sau 3s nếu không trúng
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
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnHitEnemy(Collider col)
    {
        var enemy = col.GetComponentInParent<EnemyStats>();
        if (enemy != null)
            WeaponManager.Instance.WeaponHitEnemy(enemy.gameObject, damage);
        Debug.Log($"<color=orange> FireBall hit enemy: {damage}");
    }
}