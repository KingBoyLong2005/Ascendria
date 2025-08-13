using Unity.Netcode;
using UnityEngine;

public class BulletScript : NetworkBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f;

    private Vector3 direction;
    private float timer = 0f;
    private ulong creatorClientId;
    private bool isMoving = false; // cho client-side prediction
    private bool hasDespawned = false;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
        isMoving = true;
    }

    public void SetCreator(ulong clientId)
    {
        creatorClientId = clientId;
    }

    private void Update()
    {
        // Client-side prediction: vẫn di chuyển để hình ảnh không bị trễ
        if (isMoving)
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        if (!IsServer) return;
        {
            timer += Time.deltaTime;
            if (timer >= lifeTime)
            {
                Debug.Log($"[Server] Bullet expired. Destroying. Creator: {creatorClientId}, ObjectID: {NetworkObjectId}");
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    private void DespawnBullet()
    {
        if (hasDespawned) return;
        hasDespawned = true;

        if (GetComponent<NetworkObject>().IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        GameObject other = collision.gameObject;

        // Tránh đạn va chạm với chính người bắn
        var playerHealth = other.GetComponent<PlayerHealth>();
        if (other.CompareTag("Player"))
        {
            var netObj = other.GetComponent<NetworkObject>();
            if (playerHealth != null && netObj != null && netObj.OwnerClientId != creatorClientId)
            {
                playerHealth.LoseHpServerRpc(5);
                DespawnBullet();
                return;
            }
        }

        // Nếu bắn trúng tường
        var wall = other.GetComponent<WallHealth>();
        if (other.CompareTag("Wall"))
        {
            if (wall != null)
            {
                wall.TakeDamage(1);
                DespawnBullet();
                return;
            }
        }
    }

}
