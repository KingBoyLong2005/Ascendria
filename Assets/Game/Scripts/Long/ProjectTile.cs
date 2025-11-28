using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    float damage;
    GameObject owner;

    public void Initialize(Vector3 dir, float dmg, GameObject owner)
    {
        damage = dmg; this.owner = owner;
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.linearVelocity = dir.normalized * speed;
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner) return;
        // var enemy = other.GetComponent<Enemy>();
        // if (enemy != null)
        // {
        //     // enemy.TakeDamage(damage);
        //     Destroy(gameObject);
        // }
    }
}

// using UnityEngine;

// public class Projectile : MonoBehaviour
// {
//     [Header("Projectile Stats")]
//     public float bulletSpeed = 20f;        // tốc độ đạn
//     public float damage = 20f;             // sát thương trực tiếp
//     public bool isExplosive = false;       // có nổ không
//     public float explosionRadius = 3f;     // bán kính nổ
//     public float explosionDamage = 10f;    // sát thương nổ
//     public float lifeTime = 5f;            // sống bao lâu rồi tự hủy

//     private Rigidbody rb;
//     private Vector3 moveDirection;

//     void Awake()
//     {
//         // Tự lấy Rigidbody trên prefab -> không cần kéo tay trong Inspector
//         rb = GetComponent<Rigidbody>();
//         if (rb == null)
//         {
//             Debug.LogError("Projectile không có Rigidbody!");
//         }
//     }

//     void Start()
//     {
//         // Tự hủy sau lifeTime giây nếu không trúng gì
//         Destroy(gameObject, lifeTime);
//     }

//     // Được gọi từ PlayerShooting để set hướng bay
//     public void SetDirection(Vector3 dir)
//     {
//         moveDirection = dir.normalized;

//         if (rb != null)
//         {
//             rb.linearVelocity = moveDirection * bulletSpeed;
//         }

//         // Quay đầu đạn theo hướng bay (nếu có model)
//         if (moveDirection != Vector3.zero)
//         {
//             transform.rotation = Quaternion.LookRotation(moveDirection);
//         }
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         // Kiểm tra có phải enemy không
//         Enemy enemy = other.GetComponent<Enemy>();
//         if (enemy != null)
//         {
//             // Sát thương trực tiếp
//             enemy.TakeDamage(damage);

//             // Nếu là đạn nổ -> gây damage xung quanh
//             if (isExplosive)
//             {
//                 Explode();
//             }

//             Destroy(gameObject);
//         }
//         else
//         {
//             // Nếu bạn muốn đạn chạm tường là hủy luôn
//             // Destroy(gameObject);
//         }
//     }

//     void Explode()
//     {
//         // Tìm tất cả collider trong bán kính nổ
//         Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
//         foreach (Collider hit in hits)
//         {
//             Enemy enemy = hit.GetComponent<Enemy>();
//             if (enemy != null)
//             {
//                 enemy.TakeDamage(explosionDamage);
//             }
//         }

//         // TODO: particle, sound, v.v.
//         Debug.Log("Explosion at " + transform.position);
//     }

//     private void OnDrawGizmosSelected()
//     {
//         if (isExplosive)
//         {
//             Gizmos.color = Color.red;
//             Gizmos.DrawWireSphere(transform.position, explosionRadius);
//         }
//     }
// }
