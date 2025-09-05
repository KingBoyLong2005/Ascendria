using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(NetworkObject), typeof(Collider))]
public class BulletScript : NetworkBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 5f;

    private ulong creatorClientId;
    private float timer = 0f;

    private Rigidbody rb;

    public void SetCreator(ulong clientId) => creatorClientId = clientId;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Launch(Vector3 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    private void Update()
    {
        if (!IsServer) return;

        timer += Time.deltaTime;
        if (timer >= lifeTime && GetComponent<NetworkObject>().IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // Tránh đạn va chạm với chính người bắn
        var playerHealth = collision.transform.GetComponent<PlayerHealth>();
        if (collision.transform.CompareTag("Player"))
        {
            var netObj = collision.transform.GetComponent<NetworkObject>();
            if (playerHealth != null)
            {
                if (netObj != null && netObj.OwnerClientId != creatorClientId)
                {
                    playerHealth.ApplyDamage(5, netObj.OwnerClientId);
                    GetComponent<NetworkObject>().Despawn();
                    return;
                }
                else
                {
                    Debug.LogWarning("netobj null");
                }
            }
            else
            {
                Debug.LogWarning("Playerhealth null");
            }
        }

        var enemyHealth = collision.transform.GetComponent<Enemy>();
        if (collision.transform.CompareTag("Enemy"))
        {
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1);
                return;
            }
            else
            {
                Debug.LogWarning("Enemyhealth null");
            }
        }

        // Trúng tường
        // if (other.CompareTag("Wall"))
        // {
        //     var wall = other.GetComponent<WallHealth>();
        //     if (wall != null) wall.TakeDamage(1);
        //     GetComponent<NetworkObject>().Despawn();
        // }
    }
    
}
