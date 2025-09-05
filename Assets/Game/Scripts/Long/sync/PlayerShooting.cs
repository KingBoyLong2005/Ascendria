using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform weaponRoot; // gắn từ Inspector
    [SerializeField] private float spawnOffset = 0.5f; // khoảng cách thêm trước nòng súng

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Lấy vị trí ngay nòng súng
            Vector3 spawnPos = weaponRoot.position + weaponRoot.forward * spawnOffset;
            Vector3 direction = weaponRoot.forward;

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

        bullet.SetCreator(shooterId);
        bullet.Launch(direction); // thay vì SetDirection

        netObj.Spawn();

        Debug.Log($"[SERVER] Spawned bullet (NetId:{netObj.NetworkObjectId}) by client {shooterId}");
    }

}
