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

        var rootObj = collision.transform.root;
        var netObj = rootObj.GetComponent<NetworkObject>();
        var playerHealth = rootObj.GetComponent<PlayerHealth>();

        if (rootObj.CompareTag("Player"))
        {
            if (netObj != null && playerHealth != null)
            {
                // Tránh tự bắn vào mình
                if (netObj.OwnerClientId != creatorClientId)
                {
                    playerHealth.ApplyDamage(5, creatorClientId);
                    GetComponent<NetworkObject>().Despawn();
                }
            }
            else
            {
                Debug.LogWarning("Player hoặc NetworkObject null trên root");
            }
        }
        else if (rootObj.CompareTag("Enemy"))
        {
            var enemyHealth = rootObj.GetComponent<Enemy>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamageServerRpc(1);
                GetComponent<NetworkObject>().Despawn();
            }
            else
            {
                Debug.LogWarning("EnemyHealth null");
            }
        }
    }

    
}
