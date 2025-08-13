using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float spawnOffset = 1f;

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 spawnPos = transform.position + transform.forward * spawnOffset;
            Vector3 direction = transform.forward;
            Debug.Log($"[LOCAL] Client {NetworkManager.Singleton.LocalClientId} requested shoot.");
            ShootServerRpc(spawnPos, direction);
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 spawnPos, Vector3 direction, ServerRpcParams rpcParams = default)
    {
        ulong shooterId = rpcParams.Receive.SenderClientId;
        GameObject bulletInstance = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        var netObj = bulletInstance.GetComponent<NetworkObject>();
        var bullet = bulletInstance.GetComponent<BulletScript>();

        bullet.SetDirection(direction);
        bullet.SetCreator(shooterId);

        // Spawn and make the shooter the owner of this bullet (tùy bạn có muốn).
        netObj.Spawn();

        Debug.Log($"[SERVER] Spawned bullet (NetId:{netObj.NetworkObjectId}) by client {shooterId}");
    }
}
